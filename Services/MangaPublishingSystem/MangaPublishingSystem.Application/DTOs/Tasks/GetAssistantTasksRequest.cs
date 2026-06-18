using BuildingBlocks.Web.Responses;

namespace MangaPublishingSystem.Application.DTOs.Tasks
{
    public class GetAssistantTasksRequest : PagedRequest
    {
        /// <summary>
        /// Trạng thái nhiệm vụ để lọc danh sách. Nếu để trống, hệ thống trả về toàn bộ nhiệm vụ đã được giao.
        /// Các giá trị hợp lệ: In_Progress, Submitted, Revision, Approved, Cancelled.
        /// </summary>
        public string? Status { get; set; }
    }
}
