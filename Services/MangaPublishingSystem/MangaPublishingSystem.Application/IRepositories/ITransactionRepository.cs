using System.Collections.Generic;
using System.Threading;

using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {

        // Existing method from older version
        System.Threading.Tasks.Task<IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(int walletId);
    }
}
