using System.Collections.Generic;
using System.Threading;

using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        // VNPay transaction methods
        System.Threading.Tasks.Task AddAsync(VNPayTransaction transaction, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<VNPayTransaction?> GetByReferenceAsync(string referenceCode, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task UpdateAsync(VNPayTransaction transaction, CancellationToken cancellationToken = default);

        // Existing method from older version
        System.Threading.Tasks.Task<IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(int walletId);
    }
}
