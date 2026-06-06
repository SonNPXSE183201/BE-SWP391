using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public int UserId { get; set; }
        public decimal SetupFundBalance { get; set; }
        public decimal WithdrawableBalance { get; set; }
        public decimal LockedFund { get; set; }
        public decimal LockedWithdrawable { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
