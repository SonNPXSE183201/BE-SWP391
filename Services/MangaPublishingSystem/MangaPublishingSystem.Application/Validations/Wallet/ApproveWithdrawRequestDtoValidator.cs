using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Wallet;

namespace MangaPublishingSystem.Application.Validations.Wallet
{
    public class ApproveWithdrawRequestDtoValidator : AbstractValidator<ApproveWithdrawRequestDto>
    {
        public ApproveWithdrawRequestDtoValidator()
        {
            RuleFor(x => x.AdminNote)
                .NotEmpty().WithMessage("Lý do phê duyệt hoặc từ chối là bắt buộc.")
                .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự.");
        }
    }
}
