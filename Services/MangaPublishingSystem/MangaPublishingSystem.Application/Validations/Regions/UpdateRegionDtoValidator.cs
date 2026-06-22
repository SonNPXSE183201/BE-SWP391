using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Regions;

namespace MangaPublishingSystem.Application.Validations.Regions
{
    public class UpdateRegionDtoValidator : AbstractValidator<UpdateRegionDto>
    {
        public UpdateRegionDtoValidator()
        {
            RuleFor(x => x.CoordinatesJson)
                .NotEmpty().WithMessage("Tọa độ phân vùng Canvas không được để trống.");
        }
    }
}
