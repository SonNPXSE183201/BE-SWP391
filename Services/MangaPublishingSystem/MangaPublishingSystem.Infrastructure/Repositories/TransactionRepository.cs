using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(int walletId)
        {
            return await _context.Transactions
                .Include(t => t.FromUser)
                .Include(t => t.ToUser)
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetPaymentTransactionsAsync(DateTime? from, DateTime? to, string? referenceCode)
        {
            var query = _context.Transactions
                .Include(t => t.ToUser).ThenInclude(u => u!.Role)
                .Include(t => t.FromUser).ThenInclude(u => u!.Role)
                .Include(t => t.Wallet)
                    .ThenInclude(w => w!.User)
                    .ThenInclude(u => u!.Role)
                .Where(t =>
                    t.Type == "Deposit"
                    || t.Type == "Withdrawal"
                    || t.Type == "Production_Funding"
                    || t.Type == "Platform_TopUp")
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(t => t.CreateAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(t => t.CreateAt <= to.Value);
            }

            if (!string.IsNullOrWhiteSpace(referenceCode))
            {
                var code = referenceCode.Trim();
                query = query.Where(t =>
                    (t.ReferenceCode != null && t.ReferenceCode.Contains(code)) ||
                    t.Id.ToString().Contains(code));
            }

            return await query.OrderByDescending(t => t.CreateAt).ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetPendingWithdrawalsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Wallet).ThenInclude(w => w.User).ThenInclude(u => u.Role)
                .Include(t => t.FromUser).ThenInclude(u => u!.Role)
                .Include(t => t.ToUser).ThenInclude(u => u!.Role)
                .Where(t => t.Type == "Withdrawal" && t.Status == "Pending")
                .OrderByDescending(t => t.CreateAt)
                .ToListAsync();
        }
    }
}
