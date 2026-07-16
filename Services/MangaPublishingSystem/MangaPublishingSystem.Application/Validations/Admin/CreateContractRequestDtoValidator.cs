using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Admin;

namespace MangaPublishingSystem.Application.Validations.Admin
{
    public class CreateContractRequestDtoValidator : AbstractValidator<CreateContractRequestDto>
    {
        public CreateContractRequestDtoValidator()
        {
            RuleFor(x => x.SeriesId)
                .NotEmpty().WithMessage("Series id is required.")
                .Must(id => int.TryParse(id, out var parsed) && parsed > 0)
                .WithMessage("Series id is invalid.");

            RuleFor(x => x.BaseGenkouryoPrice)
                .GreaterThan(0).WithMessage("Base genkouryo price must be greater than 0.")
                .Must(value => decimal.Round(value, 2, System.MidpointRounding.AwayFromZero) == value)
                .WithMessage("Base genkouryo price supports up to 2 decimal places.");

            RuleFor(x => x.TemplateId)
                .GreaterThanOrEqualTo(0).WithMessage("Template id is invalid.");
        }
    }
}
