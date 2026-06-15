using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.User;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateUserByAdminAsync(CreateUserByAdminDto dto);

        Task<List<AssistantResponseDto>> GetPendingAssistantsAsync();

        Task<UserResponseDto> ApproveUserAsync(int id);

        Task<UserResponseDto> RejectUserAsync(int id);

        Task<UserResponseDto> LockUserAsync(int id);

        Task<List<UserListItemDto>> GetUsersAsync(string? role, string? status);

        Task<AssistantProfileResponseDto> ApproveAssistantAsync(int id, ApproveAssistantRequestDto dto);
    }
}
