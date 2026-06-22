using System;

namespace MangaPublishingSystem.Application.DTOs.Annotations
{
    public class AnnotationDto
    {
        public int Id { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = null!;
        public string CreatedByUserRole { get; set; } = null!;
        public int? PageId { get; set; }
        public int? TaskVersionId { get; set; }
        public string CoordinatesJson { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public string? Type { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
