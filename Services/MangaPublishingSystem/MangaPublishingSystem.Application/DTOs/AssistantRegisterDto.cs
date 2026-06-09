namespace MangaPublishingSystem.Application.DTOs.User;

public class AssistantRegisterDto
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PortfolioUrl { get; set; } = null!;
    public string Skills { get; set; } = null!;
}