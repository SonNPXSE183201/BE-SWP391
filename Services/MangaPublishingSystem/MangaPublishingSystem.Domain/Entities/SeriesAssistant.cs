using System;

namespace MangaPublishingSystem.Domain.Entities
{
    /// <summary>
    /// Thành viên nhóm Assistant cố định theo Series (Flow 2 — đội ngũ dự án).
    /// </summary>
    public class SeriesAssistant
    {
        public int SeriesId { get; set; }
        public int AssistantId { get; set; }
        public string RoleInTeam { get; set; } = null!;
        public DateTime? JoinedDate { get; set; }
        /// <summary>Pending | Active | Inactive | Removed</summary>
        public string Status { get; set; } = "Pending";
        /// <summary>
        /// Khi member đã Active mà được mời thêm role mới,
        /// role mới lưu ở đây chờ assistant chấp nhận/từ chối.
        /// Null = không có lời mời role mới nào chờ xử lý.
        /// </summary>
        public string? PendingNewRoles { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public virtual Series Series { get; set; } = null!;
        public virtual User Assistant { get; set; } = null!;
    }
}
