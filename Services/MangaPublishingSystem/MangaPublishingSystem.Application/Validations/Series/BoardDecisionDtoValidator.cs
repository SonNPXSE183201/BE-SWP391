using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Series;

namespace MangaPublishingSystem.Application.Validations.Series
{
    public class BoardDecisionDtoValidator : AbstractValidator<BoardDecisionDto>
    {
        public BoardDecisionDtoValidator()
        {
            RuleFor(x => x.ApprovedProductionBudget)
                .GreaterThanOrEqualTo(0).WithMessage("Ngân sách sản xuất được phê duyệt phải lớn hơn hoặc bằng 0 VND.");

            RuleFor(x => x.PublicationSchedule)
                .NotEmpty().WithMessage("Lịch xuất bản (PublicationSchedule) không được để trống.")
                .Must(x => x == "Weekly" || x == "Monthly")
                .WithMessage("Lịch xuất bản bắt buộc phải là 'Weekly' hoặc 'Monthly'.");
        }
    }
}
