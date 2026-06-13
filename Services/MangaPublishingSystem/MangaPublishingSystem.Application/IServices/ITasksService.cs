using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface ITasksService : IGenericService<Tasks>
    {
        Task<TasksDto> CreateTaskAsync(int mangakaId, CreateTaskDto createDto);
        System.Threading.Tasks.Task ApproveSubmissionAsync(int taskId, int mangakaId, ApproveTaskDto approveDto);
        System.Threading.Tasks.Task RejectSubmissionAsync(int taskId, int mangakaId, RejectTaskDto rejectDto);
        System.Threading.Tasks.Task HandleExtensionRequestAsync(int taskId, int mangakaId, bool approve);
        System.Threading.Tasks.Task EmergencyCancelTaskAsync(int taskId, int mangakaId);
        Task<IEnumerable<TasksDto>> GetTasksByMangakaIdAsync(int mangakaId);
        Task<IEnumerable<TasksDto>> GetTasksByAssistantIdAsync(int assistantId);
        Task<IEnumerable<TaskVersionDto>> GetTaskVersionsAsync(int taskId);
        Task<byte[]> GetCompositedPageAsync(int pageId);
    }
}