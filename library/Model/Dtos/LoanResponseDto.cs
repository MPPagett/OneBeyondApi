namespace OneBeyondApi.Model.Dtos
{
    public class LoanResponseDto
    {
        public Guid BorrowerId { get; set; }
        public string BorrowerName { get; set; }
        public string BorrowerEmail { get; set; }

        public Guid BookStockId { get; set; }
        public string BookTitle { get; set; }
        public string BookISBN { get; set; }
        public DateTime? LoanEndDate { get; set; }
    }
}
