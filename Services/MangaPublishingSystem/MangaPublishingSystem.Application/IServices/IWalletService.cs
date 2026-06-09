using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IWalletService : IGenericService<Wallet>
    {
        Task<Wallet?> GetWalletByUserIdAsync(int userId);
        Task<string> DepositAsync(int userId, decimal amount);
        Task<bool> ConfirmDepositAsync(string referenceCode, string status);
        Task<Transaction> WithdrawAsync(int userId, decimal amount, string bankName, string accountNumber, string accountName);
        System.Threading.Tasks.Task LockFundsAsync(int userId, decimal amount, int taskId);
        System.Threading.Tasks.Task ReleaseFundsAsync(int taskId, bool isApproved, decimal? customPercentageForAssistant = null);
        Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int userId);
    }
}