namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class PlatformWalletDto
    {
        public decimal Balance { get; set; }
        public int TreasuryWalletId { get; set; }
        public string Label { get; set; } = "Ví quỹ chung NXB";
        public string Description { get; set; } = "Nguồn cấp vốn sản xuất cho Mangaka sau khi HĐ được ký và Mangaka xác nhận nhận vốn.";
    }

    public class TopUpPlatformWalletDto
    {
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
