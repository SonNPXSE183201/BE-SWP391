using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ITasksRepository : IGenericRepository<Tasks>
    {
        Task<PagedResult<Tasks>> GetAvailableTasksAsync(string? skill, int pageNumber, int pageSize);
        Task<PagedResult<Tasks>> GetAssistantTasksAsync(int assistantId, string? status, int pageNumber, int pageSize);
    }
}