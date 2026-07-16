namespace MangaPublishingSystem.Application.DTOs.Contracts
{
    public class UpdateContractTemplateDto
    {
        public string Content { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
