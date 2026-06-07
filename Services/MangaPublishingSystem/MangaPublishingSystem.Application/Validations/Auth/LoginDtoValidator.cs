using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Auth;

namespace MangaPublishingSystem.Application.Validations.Auth
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Username hoặc Email không được để trống.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.");
        }
    }
}
