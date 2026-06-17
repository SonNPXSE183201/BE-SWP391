namespace MangaPublishingSystem.Application.DTOs.Notifications
{
    public class WalletUpdatedPayload
    {
        public int WalletId { get; set; }
        public decimal SetupFundBalance { get; set; }
        public decimal WithdrawableBalance { get; set; }
    }
}
