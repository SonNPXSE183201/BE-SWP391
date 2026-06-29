using FluentValidation;
using MangaPublishingSystem.Application.DTOs.Series;

namespace MangaPublishingSystem.Application.Validations.Series
{
    public class CreateSeriesDtoValidator : AbstractValidator<CreateSeriesDto>
    {
        public CreateSeriesDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề bộ truyện không được để trống.")
                .MaximumLength(250).WithMessage("Tiêu đề bộ truyện không được vượt quá 250 ký tự.");

            RuleFor(x => x.Genre)
                .MaximumLength(100).WithMessage("Thể loại truyện không được vượt quá 100 ký tự.");

            RuleFor(x => x.Synopsis)
                .MaximumLength(4000).WithMessage("Tóm tắt nội dung không được vượt quá 4000 ký tự.");

            RuleFor(x => x.CoverArtworkUrl)
                .MaximumLength(500).WithMessage("Đường dẫn ảnh bìa không được vượt quá 500 ký tự.");

            RuleFor(x => x.ResourceFolderUrl)
                .MaximumLength(500).WithMessage("Đường dẫn bản phác thảo (Name) không được vượt quá 500 ký tự.");

            RuleFor(x => x.EstimatedProductionBudget)
                .GreaterThan(0).WithMessage("Ngân sách bản thảo Nemu phải lớn hơn 0 VND.");
        }
    }
}
