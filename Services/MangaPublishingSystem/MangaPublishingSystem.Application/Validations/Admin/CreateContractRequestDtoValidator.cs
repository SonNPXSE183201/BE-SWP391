using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Admin;

namespace MangaPublishingSystem.Application.Validations.Admin
{
    public class CreateContractRequestDtoValidator : AbstractValidator<CreateContractRequestDto>
    {
        public CreateContractRequestDtoValidator()
        {
            RuleFor(x => x.SeriesId)
                .NotEmpty().WithMessage("Mã series không được để trống.")
                .Must(id => int.TryParse(id, out var parsed) && parsed > 0)
                .WithMessage("Mã series không hợp lệ.");

            RuleFor(x => x.BaseGenkouryoPrice)
                .GreaterThan(0).WithMessage("Đơn giá nhuận bút phải lớn hơn 0.")
                .Must(value => decimal.Round(value, 2, System.MidpointRounding.AwayFromZero) == value)
                .WithMessage("Đơn giá nhuận bút chỉ hỗ trợ tối đa 2 chữ số thập phân.");
        }
    }
}
