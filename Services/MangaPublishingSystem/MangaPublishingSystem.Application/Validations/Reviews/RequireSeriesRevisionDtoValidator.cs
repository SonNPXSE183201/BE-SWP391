using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Reviews;

namespace MangaPublishingSystem.Application.Validations.Reviews
{
    public class RequireSeriesRevisionDtoValidator : AbstractValidator<RequireSeriesRevisionDto>
    {
        public RequireSeriesRevisionDtoValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("Dữ liệu yêu cầu không hợp lệ.");

            RuleFor(x => x.Comment)
                .NotEmpty()
                .When(x => x != null)
                .WithMessage("Nội dung yêu cầu chỉnh sửa không được để trống.");

            RuleFor(x => x.SuggestedBudget)
                .GreaterThan(0)
                .When(x => x != null && x.SuggestedBudget.HasValue)
                .WithMessage("Ngân sách đề xuất phải lớn hơn 0.");
        }
    }
}
