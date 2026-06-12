using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(MangaPublishingDbContext context) : base(context)
        {
        }


        // Existing method from older version
        public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(int walletId)
        {
            return await _context.Transactions
                .Include(t => t.FromUser)
                .Include(t => t.ToUser)
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Transaction>> GetPendingWithdrawalsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Wallet)
                .Include(t => t.FromUser)
                .Include(t => t.ToUser)
                .Where(t => t.Type == "Withdrawal" && t.Status == "Pending")
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();
        }
    }
}