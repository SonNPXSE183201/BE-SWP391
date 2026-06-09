namespace MangaPublishingSystem.Application.DTOs.User;

public class AssistantResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? PortfolioUrl { get; set; }
    public string? Skills { get; set; }
}