using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Profile;
using MangaPublishingSystem.Application.IServices.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Profile
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize]
        [HttpPut]
        public async Task<ActionResult<ApiResponse<ProfileResponseDto>>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var profile = await _profileService.UpdateMyProfileAsync(CurrentUserId, dto);
            return Ok(ApiResponse<ProfileResponseDto>.Success(profile, "Cập nhật hồ sơ cá nhân thành công."));
        }
    }
}
