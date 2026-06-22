using FluentValidation;
using MangaPublishingSystem.Application.DTOs.User;

namespace MangaPublishingSystem.Application.Validations.User
{
    public class UpdateUserByAdminDtoValidator : AbstractValidator<UpdateUserByAdminDto>
    {
        public UpdateUserByAdminDtoValidator()
        {
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

            RuleFor(x => x.PenName)
                .MaximumLength(100).WithMessage("Bút danh không được vượt quá 100 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.PenName));
        }
    }
}
