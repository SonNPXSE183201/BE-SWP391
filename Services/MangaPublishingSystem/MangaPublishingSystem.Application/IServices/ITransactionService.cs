using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs;

namespace MangaPublishingSystem.Application.IServices
{
    public interface ITransactionService : IGenericService<Transaction>
    {
        System.Threading.Tasks.Task<System.Guid> CreateDepositAsync(DepositRequestDto request);
        System.Threading.Tasks.Task HandleCallbackAsync(VnpayCallbackDto callback);
    }
}