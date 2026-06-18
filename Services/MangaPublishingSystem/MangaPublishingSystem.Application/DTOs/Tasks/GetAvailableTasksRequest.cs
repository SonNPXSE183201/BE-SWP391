using BuildingBlocks.Web.Responses;

namespace MangaPublishingSystem.Application.DTOs.Tasks
{
    public class GetAvailableTasksRequest : PagedRequest
    {
        /// <summary>
        /// Từ khóa kỹ năng để lọc tìm kiếm trong mô tả công việc (Description) của Task.
        /// Hỗ trợ tìm kiếm không phân biệt hoa thường và không phân biệt dấu tiếng Việt.
        /// </summary>
        public string? Skill { get; set; }
    }
}
