using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Reviews;

namespace MangaPublishingSystem.Application.Validations.Reviews
{
    public class RequireSeriesRevisionDtoValidator : AbstractValidator<RequireSeriesRevisionDto>
    {
        public RequireSeriesRevisionDtoValidator()
        {
            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Nội dung yêu cầu chỉnh sửa không được để trống.");
        }
    }
}
