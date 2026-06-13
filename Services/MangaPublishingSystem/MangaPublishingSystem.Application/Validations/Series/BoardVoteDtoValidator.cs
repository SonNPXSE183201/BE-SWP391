using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Series;

namespace MangaPublishingSystem.Application.Validations.Series
{
    public class BoardVoteDtoValidator : AbstractValidator<BoardVoteDto>
    {
        public BoardVoteDtoValidator()
        {
            RuleFor(x => x.VoteType)
                .NotEmpty().WithMessage("Loại bỏ phiếu (VoteType) không được để trống.")
                .Must(x => x == "Approved" || x == "Rejected")
                .WithMessage("Kết quả bỏ phiếu phải là 'Approved' hoặc 'Rejected'.");

            RuleFor(x => x.RecommendedBudget)
                .GreaterThanOrEqualTo(0).WithMessage("Ngân sách đề xuất khuyến nghị phải lớn hơn hoặc bằng 0 VND.");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Nhận xét bình luận không được vượt quá 1000 ký tự.");
        }
    }
}
