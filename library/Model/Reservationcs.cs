namespace OneBeyondApi.Model
{
    public class Reservationcs
    {
        public int Id { get; set; }
        public int BorrowerId { get; set; }
        public int BookId { get; set; }
        public DateTime ReservedDate { get; set; }

        public Borrower Borrower { get; set; } = default!;
        public Book Book { get; set; } = default!;
    }
}
