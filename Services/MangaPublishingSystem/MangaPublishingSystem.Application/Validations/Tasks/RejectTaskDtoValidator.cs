using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Tasks;

namespace MangaPublishingSystem.Application.Validations.Tasks
{
    public class RejectTaskDtoValidator : AbstractValidator<RejectTaskDto>
    {
        public RejectTaskDtoValidator()
        {
            RuleFor(x => x.FeedbackComment)
                .NotEmpty().WithMessage("Nhận xét chỉ ra điểm lỗi không được để trống.")
                .MaximumLength(1000).WithMessage("Nhận xét lỗi không được vượt quá 1000 ký tự.");

            RuleFor(x => x.RevisionExtensionHours)
                .Must(hours => hours == 24 || hours == 48)
                .WithMessage("Thời gian gia hạn thêm cho deadline để sửa tranh bắt buộc phải là 24 giờ hoặc 48 giờ.");

            RuleFor(x => x.CoordinatesJson)
                .NotEmpty().WithMessage("Tọa độ khoanh vùng lỗi trên Canvas không được để trống.");
        }
    }
}
