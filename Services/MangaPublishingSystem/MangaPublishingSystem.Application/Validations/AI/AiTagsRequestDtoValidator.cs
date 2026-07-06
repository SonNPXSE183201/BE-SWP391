using FluentValidation;
using MangaPublishingSystem.Application.DTOs.AI;

namespace MangaPublishingSystem.Application.Validations.AI
{
    public class AiTagsRequestDtoValidator : AbstractValidator<AiTagsRequestDto>
    {
        public AiTagsRequestDtoValidator()
        {
            RuleFor(x => x.Synopsis)
                .NotEmpty().WithMessage("Tóm tắt (Synopsis) không được để trống.")
                .MinimumLength(10).WithMessage("Tóm tắt phải có ít nhất 10 ký tự để AI có thể phân tích chính xác.");
        }
    }
}
