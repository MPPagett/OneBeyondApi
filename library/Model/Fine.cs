namespace OneBeyondApi.Model
{
    public class Fine
    {
        public int Id { get; set; }
        public int BorrowerId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public DateTime IssuedDate { get; set; }

        public Borrower Borrower { get; set; } = default!;
    }
}
