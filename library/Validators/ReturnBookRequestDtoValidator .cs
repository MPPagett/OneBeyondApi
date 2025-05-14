using FluentValidation;
using OneBeyondApi.Model.Dtos;

namespace OneBeyondApi.Validators
{
    public class ReturnBookRequestDtoValidator : AbstractValidator<ReturnBookRequestDto>
    {
        public ReturnBookRequestDtoValidator()
        {
            RuleFor(x => x.BookStockId).NotEmpty().WithMessage("BookStockId is required.");
        }
    }
}
