using System.Collections.Generic;
using System.Linq;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.DTOs.Series
{
    public static class SeriesMappingExtensions
    {
        public static SeriesDto ToDto(this MangaPublishingSystem.Domain.Entities.Series series)
        {
            if (series == null) return null!;

            return new SeriesDto
            {
                Id = series.Id,
                MangakaId = series.MangakaId,
                EditorId = series.EditorId,
                Title = series.Title,
                Genre = series.Genre,
                Synopsis = series.Synopsis,
                CoverArtworkUrl = series.CoverArtworkUrl,
                EstimatedProductionBudget = series.EstimatedProductionBudget,
                ApprovedProductionBudget = series.ApprovedProductionBudget,
                PublicationSchedule = series.PublicationSchedule,
                Status = series.Status,
                ResourceFolderUrl = series.ResourceFolderUrl,
                DraftManuscriptUrl = series.DraftManuscriptUrl,
                EditorReport = series.EditorReport,
                SuggestedBudget = series.SuggestedBudget,
                MangakaName = series.Mangaka?.FullName,
                EditorName = series.Editor?.FullName,
                CreateAt = series.CreateAt,
                UpdateAt = series.UpdateAt
            };
        }

        public static IEnumerable<SeriesDto> ToDtoList(this IEnumerable<MangaPublishingSystem.Domain.Entities.Series> list)
        {
            if (list == null) return Enumerable.Empty<SeriesDto>();
            return list.Select(s => s.ToDto());
        }
    }
}
