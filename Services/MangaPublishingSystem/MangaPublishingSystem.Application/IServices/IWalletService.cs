using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Wallet;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IWalletService : IGenericService<Wallet>
    {
        Task<WalletDto?> GetWalletByUserIdAsync(int userId);
        Task<string> DepositAsync(int userId, decimal amount, string ipAddr = "127.0.0.1");
        Task<TransactionDto?> GetDepositByReferenceCodeAsync(string referenceCode);
        Task<bool> ConfirmDepositAsync(string referenceCode, string status);
        Task<TransactionDto> WithdrawAsync(int userId, decimal amount, string bankName, string accountNumber, string accountName);
        Task<IEnumerable<TransactionDto>> GetPendingWithdrawalsAsync();
        Task<TransactionDto> ApproveWithdrawAsync(int transactionId, bool isApproved, string? adminNote);
        System.Threading.Tasks.Task LockFundsAsync(int userId, decimal amount, int taskId);
        System.Threading.Tasks.Task ReleaseFundsAsync(int taskId, bool isApproved, decimal? customPercentageForAssistant = null);
        Task<IEnumerable<TransactionDto>> GetTransactionHistoryAsync(int userId);
        System.Threading.Tasks.Task FundWalletAsync(int userId, decimal amount, int seriesId);
    }
}