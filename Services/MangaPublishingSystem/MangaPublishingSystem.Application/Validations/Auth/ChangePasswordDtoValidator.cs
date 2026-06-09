using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Auth;

namespace MangaPublishingSystem.Application.Validations.Auth
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải chứa ít nhất 8 ký tự.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
                .WithMessage("Mật khẩu phải bao gồm ít nhất 8 ký tự, chứa chữ hoa, chữ thường, chữ số và ký tự đặc biệt.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu mới không được để trống.")
                .Equal(x => x.NewPassword).WithMessage("Mật khẩu mới và mật khẩu xác nhận không khớp.");
        }
    }
}
