using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Admin;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardResponseDto> GetAdminDashboardAsync();
    }
}
