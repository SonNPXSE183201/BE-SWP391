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

        public async System.Threading.Tasks.Task AddAsync(VNPayTransaction transaction, System.Threading.CancellationToken cancellationToken = default)
        {
            await _context.VNPayTransactions.AddAsync(transaction, cancellationToken);
        }

        public async System.Threading.Tasks.Task<MangaPublishingSystem.Domain.Entities.VNPayTransaction?> GetByReferenceAsync(string referenceCode, System.Threading.CancellationToken cancellationToken = default)
        {
            return await _context.VNPayTransactions.FirstOrDefaultAsync(v => v.ReferenceCode == referenceCode, cancellationToken);
        }

        public async System.Threading.Tasks.Task UpdateAsync(VNPayTransaction transaction, System.Threading.CancellationToken cancellationToken = default)
        {
            _context.VNPayTransactions.Update(transaction);
            await _context.SaveChangesAsync(cancellationToken);
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
    }
}