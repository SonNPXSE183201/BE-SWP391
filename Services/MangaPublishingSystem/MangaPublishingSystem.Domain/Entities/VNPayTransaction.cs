using System;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class VNPayTransaction : BaseEntity
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal
    }

    public enum TransactionStatus
    {
        Pending,
        Success,
        Failed
    }
}
