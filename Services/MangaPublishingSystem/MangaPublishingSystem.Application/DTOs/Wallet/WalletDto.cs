using System;

namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class WalletDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal SetupFundBalance { get; set; }
        public decimal WithdrawableBalance { get; set; }
        public decimal LockedFund { get; set; }
        public decimal LockedWithdrawable { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
