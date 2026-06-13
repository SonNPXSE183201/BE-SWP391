using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Wallet;

namespace MangaPublishingSystem.Application.Validations.Wallet
{
    public class DepositRequestDtoValidator : AbstractValidator<DepositRequestDto>
    {
        public DepositRequestDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Số tiền nạp phải lớn hơn 0.")
                .Must(value => decimal.Round(value, 2, System.MidpointRounding.AwayFromZero) == value)
                .WithMessage("Số tiền nạp chỉ hỗ trợ tối đa 2 chữ số thập phân.")
                .LessThanOrEqualTo(1000000000m).WithMessage("Số tiền nạp mỗi lần không được vượt quá 1.000.000.000 VND.");
        }
    }
}
