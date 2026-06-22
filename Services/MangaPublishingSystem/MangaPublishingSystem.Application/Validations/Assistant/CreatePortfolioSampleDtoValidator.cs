using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Assistant;

namespace MangaPublishingSystem.Application.Validations.Assistant
{
    public class CreatePortfolioSampleDtoValidator : AbstractValidator<CreatePortfolioSampleDto>
    {
        public CreatePortfolioSampleDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề mẫu vẽ không được để trống.")
                .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.");

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Đường dẫn ảnh mẫu vẽ không được để trống.")
                .MaximumLength(500).WithMessage("Đường dẫn ảnh không được vượt quá 500 ký tự.");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Thể loại mẫu vẽ không được để trống.")
                .MaximumLength(100).WithMessage("Thể loại không được vượt quá 100 ký tự.");
        }
    }
}
