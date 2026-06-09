using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Auth;

namespace MangaPublishingSystem.Application.Validations.Auth
{
    public class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.")
                .MaximumLength(150).WithMessage("Email không được vượt quá 150 ký tự.");
        }
    }
}
