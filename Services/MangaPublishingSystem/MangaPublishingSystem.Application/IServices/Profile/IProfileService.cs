using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Profile;

namespace MangaPublishingSystem.Application.IServices.Profile
{
    public interface IProfileService
    {
        Task<ProfileResponseDto> GetMyProfileAsync(int userId);
        Task<ProfileResponseDto> UpdateMyProfileAsync(int userId, UpdateProfileDto dto);
    }
}
