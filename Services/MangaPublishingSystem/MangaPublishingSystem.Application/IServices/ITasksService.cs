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
        System.Threading.Tasks.Task CreateDisputeAsync(int taskId, int userId, string reason);
        System.Threading.Tasks.Task HandleExtensionRequestAsync(int taskId, int mangakaId, bool approve);
        System.Threading.Tasks.Task EmergencyCancelTaskAsync(int taskId, int mangakaId);
        Task<IEnumerable<Tasks>> GetTasksByMangakaIdAsync(int mangakaId);
        Task<IEnumerable<Tasks>> GetTasksByAssistantIdAsync(int assistantId);
        Task<IEnumerable<TaskVersion>> GetTaskVersionsAsync(int taskId);
        Task<byte[]> GetCompositedPageAsync(int pageId);
        Task<string> RefreshPageCompositeAsync(int pageId);
        Task<PagedResult<TasksDto>> GetAvailableTasksAsync(int assistantId, GetAvailableTasksRequest request);
        Task<PagedResult<TasksDto>> GetAssistantTasksAsync(int assistantId, GetAssistantTasksRequest request);
        System.Threading.Tasks.Task AcceptTaskAsync(int taskId, int assistantId);
        System.Threading.Tasks.Task SubmitTaskAsync(int taskId, int assistantId, SubmitTaskDto dto);
        System.Threading.Tasks.Task RequestExtensionAsync(int taskId, int assistantId, RequestExtensionDto dto);
        Task<TasksDto> GetTaskDetailsByIdAsync(int taskId, int userId, bool isMangaka);
    }
}

