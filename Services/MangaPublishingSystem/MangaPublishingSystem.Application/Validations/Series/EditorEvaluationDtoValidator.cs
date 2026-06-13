using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Series;

namespace MangaPublishingSystem.Application.Validations.Series
{
    public class EditorEvaluationDtoValidator : AbstractValidator<EditorEvaluationDto>
    {
        public EditorEvaluationDtoValidator()
        {
            RuleFor(x => x.EvaluationReport)
                .NotEmpty().WithMessage("Báo cáo thẩm định đánh giá không được để trống.")
                .MaximumLength(4000).WithMessage("Báo cáo thẩm định không được vượt quá 4000 ký tự.");

            RuleFor(x => x.SuggestedBudget)
                .GreaterThanOrEqualTo(0).WithMessage("Ngân sách đề xuất phải lớn hơn hoặc bằng 0 VND.");
        }
    }
}
