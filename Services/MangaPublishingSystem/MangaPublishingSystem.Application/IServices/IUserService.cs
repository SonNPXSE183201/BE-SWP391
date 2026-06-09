using MangaPublishingSystem.Application.DTOs.User;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateUserByAdminAsync(CreateUserByAdminDto dto);

        Task<AssistantResponseDto> RegisterAssistantAsync(AssistantRegisterDto dto);

        Task<List<AssistantResponseDto>> GetPendingAssistantsAsync();

        Task<AssistantResponseDto> ApproveAssistantAsync(int id);

        Task<AssistantResponseDto> RejectAssistantAsync(int id);
    }
}