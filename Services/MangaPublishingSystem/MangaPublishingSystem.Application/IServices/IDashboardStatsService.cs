using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Dashboard;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IDashboardStatsService
    {
        Task<DashboardStatsResponseDto> GetStatsAsync(int userId, string roleName);
    }
}
