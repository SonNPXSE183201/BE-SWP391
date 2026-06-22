using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
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

        Task<UserResponseDto> UnlockUserAsync(int id);

        Task<PagedResult<UserListItemDto>> GetUsersAsync(string? role, string? status, string? search, int pageNumber, int pageSize);

        Task<AssistantProfileResponseDto> ApproveAssistantAsync(int id, ApproveAssistantRequestDto dto);

        Task<UserResponseDto> UpdateUserByAdminAsync(int id, UpdateUserByAdminDto dto);
    }
}
