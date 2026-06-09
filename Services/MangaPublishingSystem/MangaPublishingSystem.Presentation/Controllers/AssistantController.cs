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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _userService.GetPendingAssistantsAsync();
            return Ok(result);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _userService.ApproveAssistantAsync(id);
            return Ok(result);
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _userService.RejectAssistantAsync(id);
            return Ok(result);
        }
    }
}