using FluentValidation;
using MangaPublishingSystem.Application.DTOs.User;

namespace MangaPublishingSystem.Application.Validations.User
{
    public class ApproveAssistantRequestDtoValidator : AbstractValidator<ApproveAssistantRequestDto>
    {
        public ApproveAssistantRequestDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Mã người dùng không được để trống.")
                .Must(id => int.TryParse(id, out var parsed) && parsed > 0)
                .WithMessage("Mã người dùng không hợp lệ.");

            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Lý do từ chối không được vượt quá 500 ký tự.")
                .When(x => !x.Approved && !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
