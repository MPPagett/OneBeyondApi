using FluentValidation;
using OneBeyondApi.Model;

namespace OneBeyondApi.Validators
{
    public class ReserveBookRequestDtoValidator : AbstractValidator<ReserveBookRequestDto>
    {
        public ReserveBookRequestDtoValidator()
        {
            RuleFor(x => x.BookId).NotEmpty().WithMessage("BookId is required.");
            RuleFor(x => x.BorrowerId).NotEmpty().WithMessage("BorrowerId is required.");
        }
    }
}
