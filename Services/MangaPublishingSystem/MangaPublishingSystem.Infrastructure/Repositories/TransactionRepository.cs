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
    }
}