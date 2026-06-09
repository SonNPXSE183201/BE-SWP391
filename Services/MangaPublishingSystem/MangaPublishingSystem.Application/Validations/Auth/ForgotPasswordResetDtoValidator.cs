using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Auth;

namespace MangaPublishingSystem.Application.Validations.Auth
{
    public class ForgotPasswordResetDtoValidator : AbstractValidator<ForgotPasswordResetDto>
    {
        public ForgotPasswordResetDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.")
                .MaximumLength(150).WithMessage("Email không được vượt quá 150 ký tự.");

            RuleFor(x => x.VerificationCode)
                .NotEmpty().WithMessage("Mã xác thực không được để trống.")
                .Matches(@"^\d{6}$").WithMessage("Mã xác thực phải gồm đúng 6 chữ số.");

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
