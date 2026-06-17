namespace MangaPublishingSystem.Application.DTOs.Contracts
{
    public class CreateContractDto
    {
        public int UserId { get; set; }
        public int SeriesId { get; set; }
        public decimal BaseGenkouryoPrice { get; set; }
    }
}
