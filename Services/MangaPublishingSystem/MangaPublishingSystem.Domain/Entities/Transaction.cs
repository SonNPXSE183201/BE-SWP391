using System;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public int WalletId { get; set; }
        public string Type { get; set; } = null!;
        public int? ReferenceId { get; set; }
        public decimal SetupFundAmount { get; set; }
        public decimal WithdrawableAmount { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ReferenceCode { get; set; }
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }

        // Thông tin ngân hàng (dùng cho giao dịch rút tiền)
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? AdminNote { get; set; }

        // Navigation properties
        public virtual Wallet Wallet { get; set; } = null!;
        public virtual User? FromUser { get; set; }
        public virtual User? ToUser { get; set; }
    }
}
