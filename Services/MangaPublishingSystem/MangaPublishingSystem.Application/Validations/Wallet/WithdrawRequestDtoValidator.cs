using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Wallet;

namespace MangaPublishingSystem.Application.Validations.Wallet
{
    public class WithdrawRequestDtoValidator : AbstractValidator<WithdrawRequestDto>
    {
        public WithdrawRequestDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Số tiền rút phải lớn hơn 0.")
                .Must(value => decimal.Round(value, 2, System.MidpointRounding.AwayFromZero) == value)
                .WithMessage("Số tiền rút chỉ hỗ trợ tối đa 2 chữ số thập phân.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Tên ngân hàng không được để trống.")
                .MaximumLength(100).WithMessage("Tên ngân hàng không được vượt quá 100 ký tự.");

            RuleFor(x => x.BankAccountNumber)
                .NotEmpty().WithMessage("Số tài khoản ngân hàng không được để trống.")
                .Matches(@"^\d{9,14}$").WithMessage("Số tài khoản ngân hàng phải từ 9 đến 14 chữ số.");

            RuleFor(x => x.BankAccountName)
                .NotEmpty().WithMessage("Tên chủ tài khoản không được để trống.")
                .Matches(@"^[A-Z\s]+$").WithMessage("Tên chủ tài khoản không được chứa ký tự đặc biệt, số và phải viết hoa không dấu.")
                .MaximumLength(150).WithMessage("Tên chủ tài khoản không được vượt quá 150 ký tự.");
        }
    }
}
