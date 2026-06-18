namespace MangaPublishingSystem.Application.DTOs.Contracts
{
    public class UpdateContractDto
    {
        public decimal BaseGenkouryoPrice { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
