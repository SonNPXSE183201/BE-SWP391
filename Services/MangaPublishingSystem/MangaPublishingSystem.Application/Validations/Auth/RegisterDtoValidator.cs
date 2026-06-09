using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Auth;

namespace MangaPublishingSystem.Application.Validations.Auth
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
                .MinimumLength(3).WithMessage("Tên đăng nhập phải chứa ít nhất 3 ký tự.")
                .MaximumLength(100).WithMessage("Tên đăng nhập không được vượt quá 100 ký tự.")
                .Matches("^[a-zA-Z0-9._]+$").WithMessage("Tên đăng nhập chỉ được phép chứa chữ cái không dấu, chữ số, dấu gạch dưới (_) hoặc dấu chấm (.).");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải chứa ít nhất 8 ký tự.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
                .WithMessage("Mật khẩu phải bao gồm ít nhất 8 ký tự, chứa chữ hoa, chữ thường, chữ số và ký tự đặc biệt.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.")
                .MaximumLength(150).WithMessage("Email không được vượt quá 150 ký tự.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên không được để trống.")
                .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự.")
                .Matches(@"^[\p{L}\p{M}\s]+$")
                .WithMessage("Họ tên chỉ được phép chứa chữ cái tiếng Việt và khoảng trắng.");

            RuleFor(x => x.PortfolioUrl)
                .Must(uri => System.Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri) 
                             && (parsedUri.Scheme == System.Uri.UriSchemeHttp || parsedUri.Scheme == System.Uri.UriSchemeHttps))
                .WithMessage("Đường dẫn Portfolio không hợp lệ (phải bắt đầu bằng http:// hoặc https://).")
                .MaximumLength(500).WithMessage("Link Portfolio không được vượt quá 500 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.PortfolioUrl));

            RuleFor(x => x.Skills)
                .MaximumLength(500).WithMessage("Thông tin kỹ năng không được vượt quá 500 ký tự.");

            RuleFor(x => x.VerificationCode)
                .Matches(@"^\d{6}$").WithMessage("Mã xác thực phải gồm đúng 6 chữ số.")
                .When(x => !string.IsNullOrEmpty(x.VerificationCode));
        }
    }
}
