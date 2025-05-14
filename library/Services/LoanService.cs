using Microsoft.EntityFrameworkCore;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;
using OneBeyondApi.Model.Dtos;
using OneBeyondApi.Services.Interfaces;

namespace OneBeyondApi.Services
{
    public class LoanService : ILoanService
    {
        private readonly LibraryContext _context;
        private readonly IFineService _fineService;

        public LoanService(LibraryContext context, IFineService fineService)
        {
            _context = context;
            _fineService = fineService;
        }

        public async Task<List<(Borrower, BookStock)>> GetBorrowersWithLoansAsync()
        {
            var test = await _context.Catalogue
                .Where(bs => bs.OnLoanTo != null)
                .Include(bs => bs.Book)
                .Include(bs => bs.OnLoanTo)
                .Select(bs => new ValueTuple<Borrower, BookStock>(bs.OnLoanTo!, bs))
                .ToListAsync();

            return test;
        }

        public async Task ReturnBookAsync(Guid bookStockId)
        {
            var bookStock = await _context.Catalogue
                .Include(bs => bs.OnLoanTo)
                .FirstOrDefaultAsync(bs => bs.Id == bookStockId);

            if (bookStock == null || bookStock.OnLoanTo == null)
                throw new ArgumentException("Book is not currently on loan.");

            var borrower = bookStock.OnLoanTo;
            var dueDate = bookStock.LoanEndDate ?? DateTime.UtcNow;

            bookStock.OnLoanTo = null;
            bookStock.LoanEndDate = null;

            if (DateTime.UtcNow > dueDate)
            {
                var overdueDays = (DateTime.UtcNow - dueDate).Days;
                var fine = new Fine
                {
                    Id = Guid.NewGuid(),
                    BorrowerId = borrower.Id,
                    Amount = overdueDays * 1.00m,
                    Reason = "Late return",
                    IssuedDate = DateTime.UtcNow
                };
                await _fineService.CreateFineAsync(fine);
            }

            await _context.SaveChangesAsync();
        }

        public async Task ReserveBookAsync(Guid bookId, Guid borrowerId)
        {
            var anyAvailable = await _context.Catalogue
                .AnyAsync(bs => bs.Book.Id == bookId && bs.OnLoanTo == null);

            if (anyAvailable)
                throw new InvalidOperationException("Book is available. No reservation needed.");

            var alreadyReserved = await _context.Reservations
                .AnyAsync(r => r.BookId == bookId && r.BorrowerId == borrowerId);

            if (alreadyReserved)
                throw new InvalidOperationException("Borrower already has a reservation for this book.");

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                BookId = bookId,
                BorrowerId = borrowerId,
                ReservedDate = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task<ReservationStatusResponseDto?> GetReservationStatusAsync(Guid bookId, Guid borrowerId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.BookId == bookId)
                .OrderBy(r => r.ReservedDate)
                .ToListAsync();

            var index = reservations.FindIndex(r => r.BorrowerId == borrowerId);
            if (index == -1)
                return null;

            var activeLoan = await _context.Catalogue
                .Where(bs => bs.Book.Id == bookId && bs.OnLoanTo != null)
                .OrderBy(bs => bs.LoanEndDate)
                .FirstOrDefaultAsync();

            DateTime? estimated = activeLoan?.LoanEndDate ?? DateTime.UtcNow;

            return new ReservationStatusResponseDto
            {
                Position = index + 1,
                EstimatedAvailability = estimated.Value.AddDays(index * 7)
            };
        }
    }
}
