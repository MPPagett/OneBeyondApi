using Microsoft.EntityFrameworkCore;
using Moq;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;
using OneBeyondApi.Services;
using OneBeyondApi.Services.Interfaces;

namespace LibraryTests.ServiceTests
{
    public class LoanServiceTests
    {
        private readonly Mock<IFineService> _fineServiceMock;

        public LoanServiceTests()
        {
            _fineServiceMock = new Mock<IFineService>();
        }

        private LibraryContext GetContext()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new LibraryContext(options);
        }

        [Fact]
        public async Task GetBorrowersWithLoans_ShouldReturnBorrowersWithBooks()
        {
            using var context = GetContext();

            var borrowerId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            context.Borrowers.Add(new Borrower
            {
                Id = borrowerId,
                Name = "Test Borrower",
                EmailAddress = "test@example.com"
            });

            context.Books.Add(new Book
            {
                Id = bookId,
                Name = "Sample Book",
                ISBN = "123456789",
                Format = BookFormat.Paperback,
                Author = new Author { Id = Guid.NewGuid(), Name = "Author A" }
            });

            await context.SaveChangesAsync();

            context.Catalogue.Add(new BookStock
            {
                Id = Guid.NewGuid(),
                Book = context.Books.First(),
                OnLoanTo = context.Borrowers.First(),
                LoanEndDate = DateTime.UtcNow.AddDays(7)
            });

            await context.SaveChangesAsync();

            var service = new LoanService(context, _fineServiceMock.Object);

            // Act
            var result = await service.GetBorrowersWithLoansAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(borrowerId, result.First().Item1.Id);
        }

        [Fact]
        public async Task ReturnBook_Late_ShouldCreateFine()
        {
            using var context = GetContext();

            var borrowerId = Guid.NewGuid();
            var bookStockId = Guid.NewGuid();

            context.Borrowers.Add(new Borrower
            {
                Id = borrowerId,
                Name = "Late User",
                EmailAddress = "late@example.com"
            });
            await context.SaveChangesAsync();

            context.Catalogue.Add(new BookStock
            {
                Id = bookStockId,
                Book = new Book
                {
                    Id = Guid.NewGuid(),
                    Name = "Late Book",
                    ISBN = "0000000",
                    Format = BookFormat.Hardback,
                    Author = new Author { Id = Guid.NewGuid(), Name = "Author B" }
                },
                OnLoanTo = context.Borrowers.First(),
                LoanEndDate = DateTime.UtcNow.AddDays(-3) // 3 days late
            });

            await context.SaveChangesAsync();

            var service = new LoanService(context, _fineServiceMock.Object);

            // Act
            await service.ReturnBookAsync(bookStockId);

            // Assert
            var updatedStock = await context.Catalogue.FindAsync(bookStockId);
            Assert.Null(updatedStock.OnLoanTo);
            Assert.Null(updatedStock.LoanEndDate);

            _fineServiceMock.Verify(f => f.CreateFineAsync(It.Is<Fine>(fine =>
                fine.BorrowerId == borrowerId &&
                fine.Amount >= 2 &&
                fine.Reason == "Late return"
                )), Times.Once);
        }

        [Fact]
        public async Task ReserveBook_WhenOnLoan_ShouldCreateReservation()
        {
            using var context = GetContext();

            var borrowerId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            var borrower = new Borrower { Id = borrowerId, Name = "Reserver", EmailAddress = "res@example.com" };
            var book = new Book
            {
                Id = bookId,
                Name = "Reserved Book",
                ISBN = "1111111111",
                Format = BookFormat.Paperback,
                Author = new Author { Id = Guid.NewGuid(), Name = "Author C" }
            };

            context.Borrowers.Add(borrower);
            context.Books.Add(book);

            context.Catalogue.Add(new BookStock
            {
                Id = Guid.NewGuid(),
                Book = book,
                OnLoanTo = new Borrower { Id = Guid.NewGuid(), Name = "Other User", EmailAddress = "other@example.com" },
                LoanEndDate = DateTime.UtcNow.AddDays(5)
            });

            await context.SaveChangesAsync();

            var service = new LoanService(context, _fineServiceMock.Object);

            // Act
            await service.ReserveBookAsync(bookId, borrowerId);

            // Assert
            var reservation = await context.Reservations.FirstOrDefaultAsync();
            Assert.NotNull(reservation);
            Assert.Equal(borrowerId, reservation.BorrowerId);
            Assert.Equal(bookId, reservation.BookId);
        }

        [Fact]
        public async Task GetReservationStatus_ShouldReturnCorrectPosition()
        {
            using var context = GetContext();

            var bookId = Guid.NewGuid();
            var borrower1Id = Guid.NewGuid();
            var borrower2Id = Guid.NewGuid();

            context.Reservations.AddRange(
                new Reservation { Id = Guid.NewGuid(), BookId = bookId, BorrowerId = borrower1Id, ReservedDate = DateTime.UtcNow },
                new Reservation { Id = Guid.NewGuid(), BookId = bookId, BorrowerId = borrower2Id, ReservedDate = DateTime.UtcNow.AddMinutes(5) }
            );

            context.Catalogue.Add(new BookStock
            {
                Id = Guid.NewGuid(),
                Book = new Book
                {
                    Id = bookId,
                    Name = "Reserved Book",
                    ISBN = "2222222222",
                    Format = BookFormat.CompactDisc,
                    Author = new Author { Id = Guid.NewGuid(), Name = "Author D" }
                },
                OnLoanTo = new Borrower { Id = Guid.NewGuid(), Name = "Current Holder", EmailAddress = "holder@example.com" },
                LoanEndDate = DateTime.UtcNow.AddDays(2)
            });

            await context.SaveChangesAsync();

            var service = new LoanService(context, _fineServiceMock.Object);

            // Act
            var status = await service.GetReservationStatusAsync(bookId, borrower2Id);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(2, status.Position);
            Assert.True(status.EstimatedAvailability > DateTime.UtcNow);
        }
    }
}