namespace MangaPublishingSystem.Application.DTOs.Contracts
{
    public class CreateContractTemplateDto
    {
        public string Content { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
