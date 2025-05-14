namespace OneBeyondApi.Model
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public Guid BorrowerId { get; set; }
        public Guid BookId { get; set; }
        public DateTime ReservedDate { get; set; }

        public Borrower Borrower { get; set; } = default!;
        public Book Book { get; set; } = default!;
    }
}
