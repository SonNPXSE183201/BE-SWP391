using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface ITasksService : IGenericService<Tasks>
    {
        Task<Tasks> CreateTaskAsync(int mangakaId, CreateTaskDto createDto);
        System.Threading.Tasks.Task ApproveSubmissionAsync(int taskId, int mangakaId, ApproveTaskDto approveDto);
        System.Threading.Tasks.Task RejectSubmissionAsync(int taskId, int mangakaId, RejectTaskDto rejectDto);
        System.Threading.Tasks.Task HandleExtensionRequestAsync(int taskId, int mangakaId, bool approve);
        System.Threading.Tasks.Task EmergencyCancelTaskAsync(int taskId, int mangakaId);
        Task<IEnumerable<Tasks>> GetTasksByMangakaIdAsync(int mangakaId);
        Task<IEnumerable<Tasks>> GetTasksByAssistantIdAsync(int assistantId);
        Task<IEnumerable<TaskVersion>> GetTaskVersionsAsync(int taskId);
        Task<byte[]> GetCompositedPageAsync(int pageId);
        Task<PagedResult<TasksDto>> GetAvailableTasksAsync(GetAvailableTasksRequest request);
        Task<PagedResult<TasksDto>> GetAssistantTasksAsync(int assistantId, GetAssistantTasksRequest request);
    }
}
