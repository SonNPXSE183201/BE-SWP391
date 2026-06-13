using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.DTOs.Contract
{
    public static class ContractMappingExtensions
    {
        public static ContractDto ToDto(this MangaPublishingSystem.Domain.Entities.Contract c)
        {
            if (c == null) return null!;

            return new ContractDto
            {
                Id = c.Id,
                UserId = c.UserId,
                SeriesId = c.SeriesId,
                BaseGenkouryoPrice = c.BaseGenkouryoPrice,
                SignedDate = c.SignedDate,
                Status = c.Status,
                MangakaName = c.User?.FullName,
                SeriesTitle = c.Series?.Title,
                CreateAt = c.CreateAt,
                UpdateAt = c.UpdateAt
            };
        }
    }
}
