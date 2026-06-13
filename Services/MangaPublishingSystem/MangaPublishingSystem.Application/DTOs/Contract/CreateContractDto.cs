namespace MangaPublishingSystem.Application.DTOs.Contract
{
    public class CreateContractDto
    {
        public int UserId { get; set; } // MangakaId
        public int SeriesId { get; set; }
        public decimal BaseGenkouryoPrice { get; set; }
    }
}
