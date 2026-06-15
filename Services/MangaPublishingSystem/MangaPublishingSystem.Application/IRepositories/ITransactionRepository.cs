using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(int walletId);

        Task<List<Transaction>> GetPaymentTransactionsAsync(DateTime? from, DateTime? to, string? referenceCode);
    }
}
