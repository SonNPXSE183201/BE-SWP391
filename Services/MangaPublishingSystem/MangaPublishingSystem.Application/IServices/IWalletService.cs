using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IWalletService : IGenericService<Wallet>
    {
        Task<Wallet?> GetWalletByUserIdAsync(int userId);
        Task<string> DepositAsync(int userId, decimal amount, string ipAddr = "127.0.0.1");
        Task<Transaction?> GetDepositByReferenceCodeAsync(string referenceCode);
        Task<bool> ConfirmDepositAsync(string referenceCode, string status);
        Task<Transaction> WithdrawAsync(int userId, decimal amount, string bankName, string accountNumber, string accountName);
        Task<IEnumerable<Transaction>> GetPendingWithdrawalsAsync();
        Task<Transaction> ApproveWithdrawAsync(int transactionId, bool isApproved, string? adminNote);
        System.Threading.Tasks.Task LockFundsAsync(int userId, decimal amount, int taskId);
        System.Threading.Tasks.Task ReleaseFundsAsync(int taskId, bool isApproved, decimal? customPercentageForAssistant = null);
        System.Threading.Tasks.Task ResolveDisputeAsync(int taskId, decimal assistantRate, int editorId);
        Task<DTOs.Wallet.ReconciliationReportDto> ReconcileTransactionsAsync(List<DTOs.Wallet.ReconciliationRow> rows);
        Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int userId);
    }
}