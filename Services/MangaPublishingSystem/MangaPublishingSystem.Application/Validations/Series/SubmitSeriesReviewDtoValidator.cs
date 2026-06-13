using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Series;

namespace MangaPublishingSystem.Application.Validations.Series
{
    public class SubmitSeriesReviewDtoValidator : AbstractValidator<SubmitSeriesReviewDto>
    {
        public SubmitSeriesReviewDtoValidator()
        {
            RuleFor(x => x.SubmissionNotes)
                .MaximumLength(1000).WithMessage("Ghi chú nộp hồ sơ không được vượt quá 1000 ký tự.");
        }
    }
}
