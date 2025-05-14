using Microsoft.EntityFrameworkCore;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;
using OneBeyondApi.Services;

namespace LibraryTests.ServiceTests
{
    public class FineServiceTests
    {
        private LibraryContext GetContext()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LibraryContext(options);
        }

        [Fact]
        public async Task CreateFine_ShouldPersistFineInDatabase()
        {
            // Arrange
            using var context = GetContext();
            var borrowerId = Guid.NewGuid();

            context.Borrowers.Add(new Borrower
            {
                Id = borrowerId,
                Name = "John Smith",
                EmailAddress = "john@example.com"
            });

            await context.SaveChangesAsync();

            var service = new FineService(context);

            var fine = new Fine
            {
                Id = Guid.NewGuid(),
                BorrowerId = borrowerId,
                Amount = 7.50m,
                Reason = "Overdue Book",
                IssuedDate = DateTime.UtcNow
            };

            // Act
            await service.CreateFineAsync(fine);

            // Assert
            var saved = await context.Fines.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal(borrowerId, saved.BorrowerId);
            Assert.Equal(7.50m, saved.Amount);
            Assert.Equal("Overdue Book", saved.Reason);
            Assert.True((DateTime.UtcNow - saved.IssuedDate).TotalSeconds < 5);
        }

        [Fact]
        public async Task CreateFine_WithZeroAmount_ShouldStillSave()
        {
            // Arrange
            using var context = GetContext();
            var borrowerId = Guid.NewGuid();

            context.Borrowers.Add(new Borrower
            {
                Id = borrowerId,
                Name = "Zero Fine User",
                EmailAddress = "zero@example.com"
            });

            await context.SaveChangesAsync();

            var service = new FineService(context);

            var fine = new Fine
            {
                Id = Guid.NewGuid(),
                BorrowerId = borrowerId,
                Amount = 0.00m,
                Reason = "Warning only",
                IssuedDate = DateTime.UtcNow
            };

            // Act
            await service.CreateFineAsync(fine);

            // Assert
            var saved = await context.Fines.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal(0.00m, saved.Amount);
            Assert.Equal("Warning only", saved.Reason);
        }

        [Fact]
        public async Task CreateFine_WithFutureDate_ShouldBeAccepted()
        {
            // Arrange
            using var context = GetContext();
            var borrowerId = Guid.NewGuid();

            context.Borrowers.Add(new Borrower
            {
                Id = borrowerId,
                Name = "Future Fine User",
                EmailAddress = "future@example.com"
            });

            await context.SaveChangesAsync();

            var service = new FineService(context);

            var futureDate = DateTime.UtcNow.AddDays(3);

            var fine = new Fine
            {
                Id = Guid.NewGuid(),
                BorrowerId = borrowerId,
                Amount = 5.00m,
                Reason = "Anticipated Overdue",
                IssuedDate = futureDate
            };

            // Act
            await service.CreateFineAsync(fine);

            // Assert
            var saved = await context.Fines.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal(futureDate.Date, saved.IssuedDate.Date);
            Assert.Equal("Anticipated Overdue", saved.Reason);
        }
    }
}
