using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
    }
}