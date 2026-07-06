using FluentValidation;
using MangaPublishingSystem.Application.DTOs.AI;

namespace MangaPublishingSystem.Application.Validations.AI
{
    public class AiAnalysisRequestDtoValidator : AbstractValidator<AiAnalysisRequestDto>
    {
        public AiAnalysisRequestDtoValidator()
        {
            RuleFor(x => x.TextContent)
                .NotEmpty().WithMessage("Nội dung văn bản không được để trống.");
        }
    }
}
