using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;
using OneBeyondApi.Model.Dtos;
using OneBeyondApi.Services.Interfaces;

namespace OneBeyondApi.Services
{
    public class LoanService : ILoanService
    {
        private readonly LibraryContext _context;

        public LoanService(LibraryContext context)
        {
            _context = context;
        }

        public Task<List<(Borrower, BookStock)>> GetBorrowersWithLoansAsync()
        {
            throw new NotImplementedException();
        }

        public Task ReturnBookAsync(Guid bookStockId)
        {
            throw new NotImplementedException();
        }

        public Task ReserveBookAsync(Guid bookId, Guid borrowerId)
        {
            throw new NotImplementedException();
        }

        public Task<Model.Dtos.ReservationStatusResponseDto?> GetReservationStatusAsync(Guid bookId, Guid borrowerId)
        {
            throw new NotImplementedException();
        }
    }
}
