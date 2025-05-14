using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.Model.Dtos;
using OneBeyondApi.Model;
using OneBeyondApi.Services.Interfaces;

namespace OneBeyondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> _logger;
        private readonly ILoanService _loanService;

        public LoanController(ILogger<LoanController> logger, ILoanService loanService)
        {
            _logger = logger;
            _loanService = loanService;
        }

        [HttpGet("OnLoan")]
        public async Task<IActionResult> GetOnLoan()
        {
            var result = await _loanService.GetBorrowersWithLoansAsync();

            var response = result.Select(r => new LoanResponseDto
            {
                BorrowerId = r.Item1.Id,
                BorrowerName = r.Item1.Name,
                BorrowerEmail = r.Item1.EmailAddress,
                BookStockId = r.Item2.Id,
                BookTitle = r.Item2.Book.Name,
                BookISBN = r.Item2.Book.ISBN,
                LoanEndDate = r.Item2.LoanEndDate
            });

            return Ok(response);
        }

        [HttpPost("ReturnBook")]
        public async Task<IActionResult> ReturnBook([FromBody] ReturnBookRequestDto request)
        {
            try
            {
                await _loanService.ReturnBookAsync(request.BookStockId);
                return Ok(new { Message = "Book returned successfully." });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to return book.");
                return StatusCode(500, new { Error = "An error occurred while returning the book." });
            }
        }

        [HttpPost("ReserveBook")]
        public async Task<IActionResult> ReserveBook([FromBody] ReserveBookRequestDto request)
        {
            try
            {
                await _loanService.ReserveBookAsync(request.BookId, request.BorrowerId);
                return Ok(new { Message = "Book reserved successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reserve book.");
                return StatusCode(500, new { Error = "An error occurred while reserving the book." });
            }
        }

        [HttpGet("ReservationStatus")]
        public async Task<IActionResult> GetReservationStatus([FromQuery] Guid bookId, [FromQuery] Guid borrowerId)
        {
            var status = await _loanService.GetReservationStatusAsync(bookId, borrowerId);

            if (status == null)
                return NotFound(new { Message = "No reservation found for this borrower and book." });

            var response = new ReservationStatusResponseDto
            {
                Position = status.Position,
                EstimatedAvailability = status.EstimatedAvailability
            };

            return Ok(response);
        }
    }
}
