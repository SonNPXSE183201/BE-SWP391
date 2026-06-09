using MangaPublishingSystem.Application.DTOs.User;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/assistants")]
    public class AssistantController : ControllerBase
    {
        private readonly IUserService _userService;

        public AssistantController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AssistantRegisterDto dto)
        {
            var result = await _userService.RegisterAssistantAsync(dto);
            return Ok(result);
        }
    }
}