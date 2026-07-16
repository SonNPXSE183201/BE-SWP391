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

            RuleFor(x => x.CitizenId)
                .Matches("^[0-9]{9,12}$").WithMessage("Số CMND/CCCD phải có từ 9 đến 12 chữ số.")
                .When(x => !string.IsNullOrWhiteSpace(x.CitizenId));

            RuleFor(x => x.CitizenIdIssueDate)
                .LessThan(System.DateTime.Today).WithMessage("Ngày cấp CMND/CCCD phải là một ngày trong quá khứ.")
                .When(x => x.CitizenIdIssueDate.HasValue);

            RuleFor(x => x.CitizenIdIssuePlace)
                .MaximumLength(200).WithMessage("Nơi cấp CMND/CCCD không được vượt quá 200 ký tự.")
                .When(x => !string.IsNullOrWhiteSpace(x.CitizenIdIssuePlace));
        }
    }
}
