namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class ReconciliationRow
    {
        public string TxnRef { get; set; } = null!;
        public decimal Amount { get; set; }
        public string ResponseCode { get; set; } = null!;
        public string PayDate { get; set; } = null!;
    }
}
