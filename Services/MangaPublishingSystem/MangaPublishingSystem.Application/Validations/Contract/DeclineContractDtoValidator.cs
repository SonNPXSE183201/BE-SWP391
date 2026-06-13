using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Contract;

namespace MangaPublishingSystem.Application.Validations.Contract
{
    public class DeclineContractDtoValidator : AbstractValidator<DeclineContractDto>
    {
        public DeclineContractDtoValidator()
        {
            RuleFor(x => x.DeclineReason)
                .NotEmpty().WithMessage("Lý do từ chối ký hợp đồng không được để trống.")
                .MaximumLength(1000).WithMessage("Lý do từ chối ký hợp đồng không được vượt quá 1000 ký tự.");
        }
    }
}
