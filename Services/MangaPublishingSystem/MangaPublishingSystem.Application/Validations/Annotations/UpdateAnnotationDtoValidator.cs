using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Annotations;

namespace MangaPublishingSystem.Application.Validations.Annotations
{
    public class UpdateAnnotationDtoValidator : AbstractValidator<UpdateAnnotationDto>
    {
        public UpdateAnnotationDtoValidator()
        {
            RuleFor(x => x.CoordinatesJson)
                .NotEmpty().WithMessage("Tọa độ chú thích không được để trống.");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Nội dung chú thích không được để trống.");
        }
    }
}
