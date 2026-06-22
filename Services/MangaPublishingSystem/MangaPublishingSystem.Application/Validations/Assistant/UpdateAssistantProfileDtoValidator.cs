using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Assistant;

namespace MangaPublishingSystem.Application.Validations.Assistant
{
    public class UpdateAssistantProfileDtoValidator : AbstractValidator<UpdateAssistantProfileDto>
    {
        public UpdateAssistantProfileDtoValidator()
        {
            RuleFor(x => x.SpecialtyTags)
                .MaximumLength(255).WithMessage("Thẻ kỹ năng chuyên môn không được vượt quá 255 ký tự.");

            RuleFor(x => x.Skills)
                .MaximumLength(500).WithMessage("Kỹ năng chi tiết không được vượt quá 500 ký tự.");

            RuleFor(x => x.PortfolioUrl)
                .MaximumLength(500).WithMessage("Liên kết Portfolio không được vượt quá 500 ký tự.");
        }
    }
}
