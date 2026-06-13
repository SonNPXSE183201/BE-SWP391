using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Tasks;

namespace MangaPublishingSystem.Application.Validations.Tasks
{
    public class ApproveTaskDtoValidator : AbstractValidator<ApproveTaskDto>
    {
        public ApproveTaskDtoValidator()
        {
            RuleFor(x => x.Rating)
                .Must(v => !v.HasValue || (v.Value >= 1 && v.Value <= 5))
                .WithMessage("Đánh giá sao nhiệm vụ phải nằm trong khoảng từ 1 đến 5 sao.");

            RuleFor(x => x.FeedbackComment)
                .MaximumLength(1000).WithMessage("Nhận xét phản hồi không được vượt quá 1000 ký tự.");
        }
    }
}
