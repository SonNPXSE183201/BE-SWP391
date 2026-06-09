namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class WithdrawRequestDto
    {
        public decimal Amount { get; set; }
        public string BankName { get; set; } = null!;
        public string BankAccountNumber { get; set; } = null!;
        public string BankAccountName { get; set; } = null!;
    }
}
