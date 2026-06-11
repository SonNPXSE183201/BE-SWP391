using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Wallet;

namespace MangaPublishingSystem.Application.Validations.Wallet
{
    public class ApproveWithdrawRequestDtoValidator : AbstractValidator<ApproveWithdrawRequestDto>
    {
        public ApproveWithdrawRequestDtoValidator()
        {
            RuleFor(x => x.TransactionId)
                .GreaterThan(0)
                .WithMessage("Mã giao dịch không hợp lệ.");

            RuleFor(x => x.AdminNote)
                .MaximumLength(500)
                .WithMessage("Ghi chú không được vượt quá 500 ký tự.");

            RuleFor(x => x.AdminNote)
                .NotEmpty()
                .When(x => !x.IsApproved)
                .WithMessage("Vui lòng nhập lý do từ chối yêu cầu rút tiền.");
        }
    }
}
