using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Admin;

namespace MangaPublishingSystem.Application.Validations.Admin
{
    public class UpdateContractRequestDtoValidator : AbstractValidator<UpdateContractRequestDto>
    {
        public UpdateContractRequestDtoValidator()
        {
            RuleFor(x => x.GenkouryoPrice)
                .GreaterThan(0).WithMessage("Đơn giá nhuận bút phải lớn hơn 0.")
                .Must(value => !value.HasValue || decimal.Round(value.Value, 2, System.MidpointRounding.AwayFromZero) == value.Value)
                .WithMessage("Đơn giá nhuận bút chỉ hỗ trợ tối đa 2 chữ số thập phân.")
                .When(x => x.GenkouryoPrice.HasValue);

            RuleFor(x => x.EndDate)
                .Must(date => string.IsNullOrWhiteSpace(date) || DateTime.TryParse(date, out _))
                .WithMessage("Ngày hiệu lực phụ lục không hợp lệ.");

            RuleFor(x => x)
                .Must(dto => dto.GenkouryoPrice.HasValue)
                .WithMessage("Phải cung cấp đơn giá nhuận bút mới khi cập nhật phụ lục hợp đồng.");
        }
    }
}
