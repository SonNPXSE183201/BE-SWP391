using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Profile;

namespace MangaPublishingSystem.Application.Validations.Profile
{
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Tên đầy đủ không được vượt quá 100 ký tự.");
            RuleFor(x => x.PenName)
                .MaximumLength(100).WithMessage("Bút danh không được vượt quá 100 ký tự.");
        }
    }
}
