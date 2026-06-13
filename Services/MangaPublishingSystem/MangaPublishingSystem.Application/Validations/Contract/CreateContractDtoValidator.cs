using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Contract;

namespace MangaPublishingSystem.Application.Validations.Contract
{
    public class CreateContractDtoValidator : AbstractValidator<CreateContractDto>
    {
        public CreateContractDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Mã người dùng (Mangaka) phải lớn hơn 0.");

            RuleFor(x => x.SeriesId)
                .GreaterThan(0).WithMessage("Mã bộ truyện phải lớn hơn 0.");

            RuleFor(x => x.BaseGenkouryoPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Giá nhuận bút cơ bản (BaseGenkouryoPrice) phải lớn hơn hoặc bằng 0 VND.");
        }
    }
}
