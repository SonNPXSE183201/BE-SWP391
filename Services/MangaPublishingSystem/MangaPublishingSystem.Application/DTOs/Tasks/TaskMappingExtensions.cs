using System.Collections.Generic;
using System.Linq;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.DTOs.Tasks
{
    public static class TaskMappingExtensions
    {
        public static TasksDto ToDto(this MangaPublishingSystem.Domain.Entities.Tasks t)
        {
            if (t == null) return null!;

            return new TasksDto
            {
                Id = t.Id,
                MangakaId = t.MangakaId,
                RegionId = t.RegionId,
                AssistantId = t.AssistantId,
                Description = t.Description,
                PaymentAmount = t.PaymentAmount,
                Deadline = t.Deadline,
                ExtensionRequestDays = t.ExtensionRequestDays,
                ExtensionReason = t.ExtensionReason,
                ExtensionStatus = t.ExtensionStatus,
                ZIndex_Order = t.ZIndex_Order,
                Status = t.Status,
                Rating = t.Rating,
                FeedbackComment = t.FeedbackComment,
                MangakaName = t.Mangaka?.FullName,
                AssistantName = t.Assistant?.FullName,
                PageNumber = t.Region?.PageId ?? 0,
                PageImageUrl = t.Region?.Page?.RawImageUrl,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            };
        }

        public static IEnumerable<TasksDto> ToDtoList(this IEnumerable<MangaPublishingSystem.Domain.Entities.Tasks> list)
        {
            if (list == null) return Enumerable.Empty<TasksDto>();
            return list.Select(t => t.ToDto());
        }

        public static TaskVersionDto ToDto(this TaskVersion v)
        {
            if (v == null) return null!;

            return new TaskVersionDto
            {
                Id = v.Id,
                TaskId = v.TaskId,
                VersionNumber = v.VersionNumber,
                SubmittedFileUrl = v.SubmittedFileUrl,
                Status = v.Status,
                SubmittedAt = v.SubmittedAt
            };
        }

        public static IEnumerable<TaskVersionDto> ToDtoList(this IEnumerable<TaskVersion> list)
        {
            if (list == null) return Enumerable.Empty<TaskVersionDto>();
            return list.Select(v => v.ToDto());
        }
    }
}
