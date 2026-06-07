using System.Threading;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        System.Threading.Tasks.Task AddAsync(VNPayTransaction transaction, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task<VNPayTransaction?> GetByReferenceAsync(string referenceCode, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task UpdateAsync(VNPayTransaction transaction, CancellationToken cancellationToken = default);
    }
}
