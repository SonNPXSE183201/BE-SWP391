using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Wallet;

namespace MangaPublishingSystem.Application.Validations.Wallet
{
    public class WithdrawRequestDtoValidator : AbstractValidator<WithdrawRequestDto>
    {
        public WithdrawRequestDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(10000).WithMessage("Số tiền rút tối thiểu là 10,000 VND.")
                .Must(value => decimal.Round(value, 2, System.MidpointRounding.AwayFromZero) == value)
                .WithMessage("Số tiền rút chỉ hỗ trợ tối đa 2 chữ số thập phân.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Tên ngân hàng không được để trống.")
                .MaximumLength(100).WithMessage("Tên ngân hàng không được vượt quá 100 ký tự.");

            RuleFor(x => x.BankAccountNumber)
                .NotEmpty().WithMessage("Số tài khoản ngân hàng không được để trống.")
                .MinimumLength(5).WithMessage("Số tài khoản ngân hàng không hợp lệ.")
                .MaximumLength(50).WithMessage("Số tài khoản ngân hàng không được vượt quá 50 ký tự.");

            RuleFor(x => x.BankAccountName)
                .NotEmpty().WithMessage("Tên chủ tài khoản không được để trống.")
                .MaximumLength(150).WithMessage("Tên chủ tài khoản không được vượt quá 150 ký tự.");
        }
    }
}
