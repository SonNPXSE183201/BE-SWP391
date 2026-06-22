using System;

namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public string Type { get; set; } = null!;
        public int? ReferenceId { get; set; }
        public decimal SetupFundAmount { get; set; }
        public decimal WithdrawableAmount { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public string? ReferenceCode { get; set; }
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public string? FromUserName { get; set; }
        public string? FromUserFullName { get; set; }
        public string? ToUserName { get; set; }
        public string? ToUserFullName { get; set; }
        public string? RequesterRole { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? AdminNote { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
