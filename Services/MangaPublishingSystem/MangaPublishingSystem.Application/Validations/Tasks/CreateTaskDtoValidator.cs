using System;
using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Tasks;

namespace MangaPublishingSystem.Application.Validations.Tasks
{
    public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskDtoValidator()
        {
            RuleFor(x => x.RegionId)
                .GreaterThan(0).WithMessage("Mã phân vùng vẽ phải lớn hơn 0.");

            RuleFor(x => x.PaymentAmount)
                .GreaterThan(0).WithMessage("Số tiền thanh toán phải lớn hơn 0 VND.");

            RuleFor(x => x.Deadline)
                .GreaterThan(DateTime.UtcNow).WithMessage("Hạn chót thực hiện phải là một thời điểm trong tương lai.");

            RuleFor(x => x.ZIndex_Order)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự lớp ảnh Z-Index không được phép âm.");
        }
    }
}
