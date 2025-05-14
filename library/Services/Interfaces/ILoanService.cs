using OneBeyondApi.Model;
using OneBeyondApi.Model.Dtos;

namespace OneBeyondApi.Services.Interfaces
{
    public interface ILoanService
    {
        Task<List<(Borrower, BookStock)>> GetBorrowersWithLoansAsync();
        Task ReturnBookAsync(Guid bookStockId);
        Task ReserveBookAsync(Guid bookId, Guid borrowerId);
        Task<Model.Dtos.ReservationStatusResponseDto?> GetReservationStatusAsync(Guid bookId, Guid borrowerId);
    }
}
