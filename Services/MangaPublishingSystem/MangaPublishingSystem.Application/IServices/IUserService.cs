using MangaPublishingSystem.Application.DTOs.User;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateUserByAdminAsync(CreateUserByAdminDto dto);
    }
}