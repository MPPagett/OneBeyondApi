namespace OneBeyondApi.Model
{
    public class ReserveBookRequestDto
    {
        public Guid BookId { get; set; }
        public Guid BorrowerId { get; set; }
    }
}
