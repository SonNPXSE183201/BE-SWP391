using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Contracts;

namespace MangaPublishingSystem.Application.Validations.Contracts
{
    public class CreateContractTemplateDtoValidator : AbstractValidator<CreateContractTemplateDto>
    {
        public CreateContractTemplateDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Nội dung mẫu hợp đồng không được để trống.");
        }
    }
}
