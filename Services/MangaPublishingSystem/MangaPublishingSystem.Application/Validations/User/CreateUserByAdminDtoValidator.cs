using FluentValidation;
using MangaPublishingSystem.Application.DTOs.User;

namespace MangaPublishingSystem.Application.Validations.User
{
    public class CreateUserByAdminDtoValidator : AbstractValidator<CreateUserByAdminDto>
    {
        public CreateUserByAdminDtoValidator()
        {
            RuleFor(x => x.RoleId)
                .Must(roleId => roleId == 2 || roleId == 3 || roleId == 4)
                .WithMessage("Vai trò không hợp lệ. Chỉ được phép tạo tài khoản cho Biên tập viên (2), Hội đồng (3) hoặc Tác giả (4).");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
                .MinimumLength(3).WithMessage("Tên đăng nhập phải chứa ít nhất 3 ký tự.")
                .MaximumLength(100).WithMessage("Tên đăng nhập không được vượt quá 100 ký tự.")
                .Matches("^[a-zA-Z0-9._]+$").WithMessage("Tên đăng nhập chỉ được phép chứa chữ cái không dấu, chữ số, dấu gạch dưới (_) hoặc dấu chấm (.).");

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
                .NotEmpty().WithMessage("Bút danh không được để trống khi tạo tài khoản Tác giả.")
                .MaximumLength(100).WithMessage("Bút danh không được vượt quá 100 ký tự.")
                .When(x => x.RoleId == 4);

            RuleFor(x => x.PenName)
                .MaximumLength(100).WithMessage("Bút danh không được vượt quá 100 ký tự.")
                .When(x => x.RoleId != 4 && !string.IsNullOrEmpty(x.PenName));

            RuleFor(x => x.CitizenId)
                .NotEmpty().WithMessage("Số CMND/CCCD không được để trống khi tạo tài khoản Tác giả.")
                .Matches("^[0-9]{9,12}$").WithMessage("Số CMND/CCCD phải chứa từ 9 đến 12 chữ số.")
                .When(x => x.RoleId == 4);

            RuleFor(x => x.CitizenId)
                .Matches("^[0-9]{9,12}$").WithMessage("Số CMND/CCCD phải chứa từ 9 đến 12 chữ số.")
                .When(x => x.RoleId != 4 && !string.IsNullOrEmpty(x.CitizenId));

            RuleFor(x => x.CitizenIdIssueDate)
                .NotNull().WithMessage("Ngày cấp CMND/CCCD không được để trống khi tạo tài khoản Tác giả.")
                .LessThan(System.DateTime.UtcNow).WithMessage("Ngày cấp CMND/CCCD không hợp lệ (phải trong quá khứ).")
                .When(x => x.RoleId == 4);

            RuleFor(x => x.CitizenIdIssueDate)
                .LessThan(System.DateTime.UtcNow).WithMessage("Ngày cấp CMND/CCCD không hợp lệ (phải trong quá khứ).")
                .When(x => x.RoleId != 4 && x.CitizenIdIssueDate.HasValue);

            RuleFor(x => x.CitizenIdIssuePlace)
                .NotEmpty().WithMessage("Nơi cấp CMND/CCCD không được để trống khi tạo tài khoản Tác giả.")
                .MaximumLength(200).WithMessage("Nơi cấp CMND/CCCD không được vượt quá 200 ký tự.")
                .When(x => x.RoleId == 4);

            RuleFor(x => x.CitizenIdIssuePlace)
                .MaximumLength(200).WithMessage("Nơi cấp CMND/CCCD không được vượt quá 200 ký tự.")
                .When(x => x.RoleId != 4 && !string.IsNullOrEmpty(x.CitizenIdIssuePlace));
        }
    }
}
