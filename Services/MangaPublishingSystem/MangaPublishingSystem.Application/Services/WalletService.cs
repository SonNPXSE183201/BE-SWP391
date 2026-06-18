using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class WalletService : GenericService<Wallet>, IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVnPayService _vnPayService;

        public WalletService(
            IWalletRepository repository,
            IUnitOfWork unitOfWork,
            ITransactionRepository transactionRepository,
            ITasksRepository tasksRepository,
            IUserRepository userRepository,
            IVnPayService vnPayService)
            : base(repository, unitOfWork)
        {
            _walletRepository = repository;
            _transactionRepository = transactionRepository;
            _tasksRepository = tasksRepository;
            _userRepository = userRepository;
            _vnPayService = vnPayService;
        }

        public async Task<Wallet?> GetWalletByUserIdAsync(int userId)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                throw new NotFoundException("Ví của người dùng không tồn tại trên hệ thống.");
            }
            return wallet;
        }

        public async Task<string> DepositAsync(int userId, decimal amount, string ipAddr = "127.0.0.1")
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                throw new NotFoundException("Ví của người dùng không tồn tại.");
            }

            if (amount < 10000)
            {
                throw new BadRequestException("Số tiền nạp tối thiểu là 10,000 VND.");
            }

            var referenceCode = "DEP" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + userId;

            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Type = "Deposit",
                Amount = amount,
                WithdrawableAmount = amount,
                Status = "Pending",
                ReferenceCode = referenceCode,
                FromUserId = userId
            };

            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            var orderInfo = $"Nap tien vi he thong cho nguoi dung {userId}";
            var paymentUrl = _vnPayService.BuildPaymentUrl(referenceCode, amount, ipAddr, orderInfo);
            return paymentUrl;
        }

        public async Task<bool> ConfirmDepositAsync(string referenceCode, string status)
        {
            var transactions = await _transactionRepository.FindAsync(t => t.ReferenceCode == referenceCode);
            var transaction = transactions.FirstOrDefault();
            if (transaction == null)
            {
                throw new NotFoundException("Giao dịch nạp tiền không tồn tại.");
            }

            if (transaction.Status != "Pending")
            {
                return transaction.Status == "Success";
            }

            if (status.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                transaction.Status = "Success";
                var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
                if (wallet != null)
                {
                    wallet.WithdrawableBalance += transaction.Amount;
                    _walletRepository.Update(wallet);
                }
            }
            else
            {
                transaction.Status = "Failed";
            }

            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            return transaction.Status == "Success";
        }

        /// <summary>
        /// Tìm giao dịch nạp tiền theo mã tham chiếu — không ném exception, trả null nếu không tìm thấy.
        /// Dùng cho IPN để kiểm tra trước khi confirm.
        /// </summary>
        public async Task<Transaction?> GetDepositByReferenceCodeAsync(string referenceCode)
        {
            var transactions = await _transactionRepository.FindAsync(
                t => t.ReferenceCode == referenceCode && t.Type == "Deposit");
            return transactions.FirstOrDefault();
        }


        public async Task<Transaction> WithdrawAsync(int userId, decimal amount, string bankName, string accountNumber, string accountName)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                throw new NotFoundException("Ví của người dùng không tồn tại.");
            }

            if (amount < 10000)
            {
                throw new BadRequestException("Số tiền rút tối thiểu là 10,000 VND.");
            }

            if (wallet.WithdrawableBalance < amount)
            {
                throw new BadRequestException("Số dư khả dụng không đủ để thực hiện rút tiền.");
            }

            wallet.WithdrawableBalance -= amount;
            wallet.LockedWithdrawable += amount;

            _walletRepository.Update(wallet);

            var referenceCode = "WDR" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + userId;
            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Type = "Withdrawal",
                Amount = amount,
                WithdrawableAmount = -amount,
                Status = "Pending", // Giao dịch chờ duyệt
                ReferenceCode = referenceCode,
                ToUserId = userId,
                BankName = bankName,
                BankAccountNumber = accountNumber,
                BankAccountName = accountName
            };

            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetPendingWithdrawalsAsync()
        {
            return await _transactionRepository.GetPendingWithdrawalsAsync();
        }

        public async Task<Transaction> ApproveWithdrawAsync(int transactionId, bool isApproved, string? adminNote)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null || transaction.Type != "Withdrawal")
            {
                throw new NotFoundException("Không tìm thấy giao dịch rút tiền hợp lệ.");
            }

            if (transaction.Status != "Pending")
            {
                throw new BadRequestException("Giao dịch này đã được xử lý.");
            }

            var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
            if (wallet == null)
            {
                throw new NotFoundException("Không tìm thấy ví liên kết với giao dịch.");
            }

            if (isApproved)
            {
                // Phê duyệt: trừ hẳn tiền khỏi LockedWithdrawable
                wallet.LockedWithdrawable -= transaction.Amount;
                transaction.Status = "Success";
            }
            else
            {
                // Từ chối: hoàn tiền từ LockedWithdrawable về WithdrawableBalance
                wallet.LockedWithdrawable -= transaction.Amount;
                wallet.WithdrawableBalance += transaction.Amount;
                transaction.Status = "Failed";
                // Đảo ngược lại Amount trong transaction record để báo cáo khớp
                transaction.WithdrawableAmount = 0; // Không trừ nữa
            }

            transaction.AdminNote = adminNote;
            transaction.UpdateAt = DateTime.UtcNow;

            _walletRepository.Update(wallet);
            _transactionRepository.Update(transaction);
            await _unitOfWork.SaveChangesAsync();

            return transaction;
        }

        public async System.Threading.Tasks.Task LockFundsAsync(int userId, decimal amount, int taskId)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                throw new NotFoundException("Ví của người dùng không tồn tại.");
            }

            if (wallet.SetupFundBalance + wallet.WithdrawableBalance < amount)
            {
                throw new BadRequestException("Số dư ví không đủ để ký quỹ cho nhiệm vụ này.");
            }

            decimal setupFundDeduct = 0;
            decimal withdrawableDeduct = 0;

            if (wallet.SetupFundBalance >= amount)
            {
                setupFundDeduct = amount;
            }
            else
            {
                setupFundDeduct = wallet.SetupFundBalance;
                withdrawableDeduct = amount - wallet.SetupFundBalance;
            }

            wallet.SetupFundBalance -= setupFundDeduct;
            wallet.WithdrawableBalance -= withdrawableDeduct;
            wallet.LockedFund += setupFundDeduct;
            wallet.LockedWithdrawable += withdrawableDeduct;

            _walletRepository.Update(wallet);

            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Type = "Escrow_Lock",
                ReferenceId = taskId,
                SetupFundAmount = -setupFundDeduct,
                WithdrawableAmount = -withdrawableDeduct,
                Amount = amount,
                Status = "Success",
                FromUserId = userId
            };

            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task ReleaseFundsAsync(int taskId, bool isApproved, decimal? customPercentageForAssistant = null)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Không tìm thấy nhiệm vụ tương ứng.");
            }

            var wallet = await _walletRepository.GetWalletByUserIdAsync(task.MangakaId);
            if (wallet == null)
            {
                throw new NotFoundException("Không tìm thấy ví của tác giả.");
            }

            // Tìm giao dịch khóa quỹ gốc
            var lockTxs = await _transactionRepository.FindAsync(t => t.Type == "Escrow_Lock" && t.ReferenceId == taskId && t.Status == "Success");
            var lockTx = lockTxs.FirstOrDefault();
            if (lockTx == null)
            {
                throw new NotFoundException("Không tìm thấy giao dịch ký quỹ hợp lệ của nhiệm vụ này.");
            }

            decimal setupFundLocked = -lockTx.SetupFundAmount;
            decimal withdrawableLocked = -lockTx.WithdrawableAmount;

            // Giải phóng từ phần ví đang khóa của Mangaka
            wallet.LockedFund = Math.Max(0, wallet.LockedFund - setupFundLocked);
            wallet.LockedWithdrawable = Math.Max(0, wallet.LockedWithdrawable - withdrawableLocked);

            if (isApproved)
            {
                if (!task.AssistantId.HasValue)
                {
                    throw new BadRequestException("Nhiệm vụ chưa được giao cho trợ lý nào.");
                }

                var assistantWallet = await _walletRepository.GetWalletByUserIdAsync(task.AssistantId.Value);
                if (assistantWallet == null)
                {
                    throw new NotFoundException("Không tìm thấy ví của trợ lý.");
                }

                decimal assistantPayment = task.PaymentAmount;
                decimal mangakaRefund = 0;

                if (customPercentageForAssistant.HasValue)
                {
                    var assistantPercent = customPercentageForAssistant.Value;
                    if (assistantPercent < 0 || assistantPercent > 100)
                    {
                        throw new BadRequestException("Tỷ lệ phân chia tiền không hợp lệ.");
                    }
                    assistantPayment = task.PaymentAmount * (assistantPercent / 100m);
                    mangakaRefund = task.PaymentAmount - assistantPayment;
                }

                // Trả phần tiền thừa (nếu có) về cho Mangaka theo tỷ lệ nguồn tiền bị khóa ban đầu (T10)
                if (mangakaRefund > 0)
                {
                    decimal ratio = mangakaRefund / task.PaymentAmount;
                    decimal setupRefund = setupFundLocked * ratio;
                    decimal withdrawableRefund = withdrawableLocked * ratio;

                    wallet.SetupFundBalance += setupRefund;
                    wallet.WithdrawableBalance += withdrawableRefund;

                    var refundTx = new Transaction
                    {
                        WalletId = wallet.Id,
                        Type = "Escrow_Refund",
                        ReferenceId = taskId,
                        SetupFundAmount = setupRefund,
                        WithdrawableAmount = withdrawableRefund,
                        Amount = mangakaRefund,
                        Status = "Success",
                        ToUserId = task.MangakaId
                    };
                    await _transactionRepository.AddAsync(refundTx);
                }

                // Cộng tiền cho Assistant
                assistantWallet.WithdrawableBalance += assistantPayment;
                _walletRepository.Update(assistantWallet);

                var assistantTx = new Transaction
                {
                    WalletId = assistantWallet.Id,
                    Type = "Task_Payment",
                    ReferenceId = taskId,
                    WithdrawableAmount = assistantPayment,
                    Amount = assistantPayment,
                    Status = "Success",
                    FromUserId = task.MangakaId,
                    ToUserId = task.AssistantId
                };
                await _transactionRepository.AddAsync(assistantTx);

                // Ghi nhận giao dịch giải ngân ví Mangaka
                var releaseTx = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = "Escrow_Release",
                    ReferenceId = taskId,
                    SetupFundAmount = -setupFundLocked,
                    WithdrawableAmount = -withdrawableLocked,
                    Amount = assistantPayment,
                    Status = "Success",
                    FromUserId = task.MangakaId,
                    ToUserId = task.AssistantId
                };
                await _transactionRepository.AddAsync(releaseTx);
            }
            else
            {
                // Hoàn trả toàn bộ tiền về ví Mangaka theo đúng ví nguồn gốc ban đầu (T10)
                wallet.SetupFundBalance += setupFundLocked;
                wallet.WithdrawableBalance += withdrawableLocked;

                var refundTx = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = "Escrow_Refund",
                    ReferenceId = taskId,
                    SetupFundAmount = setupFundLocked,
                    WithdrawableAmount = withdrawableLocked,
                    Amount = task.PaymentAmount,
                    Status = "Success",
                    ToUserId = task.MangakaId
                };
                await _transactionRepository.AddAsync(refundTx);
            }

            _walletRepository.Update(wallet);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int userId)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return Enumerable.Empty<Transaction>();
            }

            return await _transactionRepository.GetTransactionsByWalletIdAsync(wallet.Id);
        }
    }
}