using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services.Admin
{
    public class AdminReconciliationService : IAdminReconciliationService
    {
        private readonly ITransactionRepository _transactionRepository;

        public AdminReconciliationService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<ReconciliationResponseDto> GetReconciliationAsync(
            DateTime? from,
            DateTime? to,
            string? status,
            string? referenceCode)
        {
            DateTime? toInclusive = to?.Date.AddDays(1).AddTicks(-1);

            var transactions = await _transactionRepository.GetPaymentTransactionsAsync(from, toInclusive, referenceCode);

            var records = transactions.Select(MapRecord).ToList();

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                records = records.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return new ReconciliationResponseDto
            {
                Records = records,
                Summary = BuildSummary(records)
            };
        }

        private static ReconciliationRecordDto MapRecord(Domain.Entities.Transaction tx)
        {
            var userName = tx.ToUser?.FullName ?? tx.FromUser?.FullName ?? tx.Wallet?.User?.FullName ?? "N/A";
            var internalStatus = MapInternalStatus(tx.Status);
            var reference = string.IsNullOrWhiteSpace(tx.ReferenceCode) ? $"TXN-{tx.Id:D6}" : tx.ReferenceCode;
            var hasReferenceCode = !string.IsNullOrWhiteSpace(tx.ReferenceCode);
            var vnpayId = reference.StartsWith("VNP", StringComparison.OrdinalIgnoreCase) ? reference : $"VNP{tx.Id:D8}";

            var reconciliationStatus = !hasReferenceCode && (tx.Type == "Deposit" || tx.Type == "Withdraw")
                ? "Missing"
                : internalStatus switch
                {
                    "Completed" or "Success" => "Matched",
                    "Pending" => "Pending",
                    "Failed" => "Mismatch",
                    _ => "Pending"
                };

            return new ReconciliationRecordDto
            {
                Id = tx.Id.ToString(),
                ReferenceCode = reference,
                VnpayTransactionId = vnpayId,
                InternalTransactionId = $"TXN-{tx.Id:D6}",
                VnpayAmount = tx.Amount,
                InternalAmount = tx.Amount,
                VnpayDate = tx.CreateAt.ToUniversalTime().ToString("o"),
                InternalDate = tx.CreateAt.ToUniversalTime().ToString("o"),
                VnpayStatus = internalStatus == "Completed" ? "Success" : internalStatus,
                InternalStatus = internalStatus,
                Status = reconciliationStatus,
                UserName = userName,
                Description = tx.Type == "Deposit" ? "Nạp tiền vào ví — Deposit" : "Rút tiền — Withdrawal",
                DiscrepancyNote = reconciliationStatus switch
                {
                    "Mismatch" => "Trạng thái giao dịch nội bộ không khớp đối soát VNPay.",
                    "Missing" => "Giao dịch thiếu mã tham chiếu ReferenceCode theo quy tắc F04.",
                    _ => null
                }
            };
        }

        private static string MapInternalStatus(string status)
        {
            return status switch
            {
                "Success" => "Completed",
                "Failed" => "Failed",
                _ => status
            };
        }

        private static ReconciliationSummaryDto BuildSummary(List<ReconciliationRecordDto> records)
        {
            var totalVnpay = records.Sum(r => r.VnpayAmount);
            var totalInternal = records.Sum(r => r.InternalAmount);

            return new ReconciliationSummaryDto
            {
                TotalRecords = records.Count,
                MatchedCount = records.Count(r => r.Status == "Matched"),
                MismatchCount = records.Count(r => r.Status == "Mismatch"),
                MissingCount = records.Count(r => r.Status == "Missing"),
                PendingCount = records.Count(r => r.Status == "Pending"),
                TotalVnpayAmount = totalVnpay,
                TotalInternalAmount = totalInternal,
                DifferenceAmount = Math.Abs(totalVnpay - totalInternal)
            };
        }
    }
}
