using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Rankings;

namespace MangaPublishingSystem.Application.Validations.Rankings
{
    public class VoteRankingRequestDtoValidator : AbstractValidator<VoteRankingRequestDto>
    {
        public VoteRankingRequestDtoValidator()
        {
            RuleFor(x => x.SeriesId)
                .GreaterThan(0).WithMessage("Id bộ truyện không hợp lệ.");

            RuleFor(x => x.VoteType)
                .NotEmpty().WithMessage("Loại bình chọn không được để trống.")
                .Must(v => v == "Maintain" || v == "Cancel")
                .WithMessage("Loại bình chọn phải là 'Maintain' hoặc 'Cancel'.");
        }
    }
}
