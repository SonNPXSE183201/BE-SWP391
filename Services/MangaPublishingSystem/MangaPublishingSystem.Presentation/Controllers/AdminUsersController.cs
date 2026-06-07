using MangaPublishingSystem.Application.DTOs.User;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminUsersController(IUserService userService)
        {
            _userService = userService;
        }
[HttpPost]
public async Task<IActionResult> Create(CreateUserByAdminDto dto)
{
    var result = await _userService.CreateUserByAdminAsync(dto);
    return Ok(result);
}
}
}