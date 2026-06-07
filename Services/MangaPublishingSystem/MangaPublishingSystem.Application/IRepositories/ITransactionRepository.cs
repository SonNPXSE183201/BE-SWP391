using System.Threading;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ITransactionRepository
    {
        Task AddAsync(VNPayTransaction transaction, CancellationToken cancellationToken = default);
        Task<VNPayTransaction?> GetByReferenceAsync(string referenceCode, CancellationToken cancellationToken = default);
        Task UpdateAsync(VNPayTransaction transaction, CancellationToken cancellationToken = default);
    }
}
