using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Annotations;

namespace MangaPublishingSystem.Application.Validations.Annotations
{
    public class CreateAnnotationDtoValidator : AbstractValidator<CreateAnnotationDto>
    {
        public CreateAnnotationDtoValidator()
        {
            RuleFor(x => x.CoordinatesJson)
                .NotEmpty().WithMessage("Tọa độ chú thích không được để trống.");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Nội dung chú thích không được để trống.");

            RuleFor(x => x)
                .Must(x => x.PageId.HasValue || x.TaskVersionId.HasValue)
                .WithMessage("Chú thích phải gắn liền với trang truyện hoặc phiên bản nhiệm vụ.");
        }
    }
}
