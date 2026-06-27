using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.DTOs.Wallet;

namespace MangaPublishingSystem.Application.Services
{
    public class WalletService : GenericService<Wallet>, IWalletService
    {
        private const int RoleIdSystemAdmin = 1;
        private const int RoleIdAssistant = 5;

        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVnPayService _vnPayService;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IDisputeLogRepository _disputeLogRepository;
        private readonly ITaskVersionRepository _taskVersionRepository;
        private readonly IAnnotationRepository _annotationRepository;
        private readonly IRegionRepository _regionRepository;

        public WalletService(
            IWalletRepository repository,
            IUnitOfWork unitOfWork,
            ITransactionRepository transactionRepository,
            ITasksRepository tasksRepository,
            IUserRepository userRepository,
            IVnPayService vnPayService,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IDisputeLogRepository disputeLogRepository,
            ITaskVersionRepository taskVersionRepository,
            IAnnotationRepository annotationRepository,
            IRegionRepository regionRepository)
            : base(repository, unitOfWork)
        {
            _walletRepository = repository;
            _transactionRepository = transactionRepository;
            _tasksRepository = tasksRepository;
            _userRepository = userRepository;
            _vnPayService = vnPayService;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _disputeLogRepository = disputeLogRepository;
            _taskVersionRepository = taskVersionRepository;
            _annotationRepository = annotationRepository;
            _regionRepository = regionRepository;
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

            if (transaction.Status == "Success")
            {
                var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
                var notifyUserId = transaction.Type == "Platform_TopUp" && transaction.FromUserId.HasValue
                    ? transaction.FromUserId.Value
                    : wallet?.UserId;

                if (notifyUserId.HasValue)
                {
                    var isPlatformTopUp = transaction.Type == "Platform_TopUp";
                    await PublishWalletNotificationAsync(
                        notifyUserId.Value,
                        isPlatformTopUp ? "Wallet_Platform_TopUp_Success" : "Wallet_Deposit_Success",
                        isPlatformTopUp ? "Nạp quỹ NXB thành công" : "Nạp tiền thành công",
                        isPlatformTopUp
                            ? $"Quỹ NXB đã nhận {transaction.Amount:N0} VND qua VNPay. Mã GD: {referenceCode}."
                            : $"Bạn đã nạp thành công {transaction.Amount:N0} VND vào quỹ khả dụng. Mã GD: {referenceCode}.",
                        isPlatformTopUp ? "/admin/reconciliation" : null);
                }
            }

            return transaction.Status == "Success";
        }

        private async System.Threading.Tasks.Task PublishWalletNotificationAsync(
            int userId,
            string type,
            string title,
            string content,
            string? link = null)
        {
            var resolvedLink = link ?? await ResolveWalletLinkForUserAsync(userId);
            var notif = new Notification
            {
                UserId = userId,
                Content = content,
                Type = type,
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync();

            await _notificationPublisher.PublishNotificationPayloadAsync(userId, new NotificationPayload
            {
                Id = notif.Id,
                Title = title,
                Message = content,
                Link = resolvedLink,
                Type = type,
                CreateAt = notif.CreateAt
            });
        }

        private async Task<string> ResolveWalletLinkForUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.RoleId == RoleIdAssistant ? "/assistant/wallet" : "/mangaka/wallet";
        }

        private async System.Threading.Tasks.Task NotifyAdminsOfPendingWithdrawalAsync(Transaction transaction)
        {
            var admins = await _userRepository.FindAsync(u =>
                u.RoleId == RoleIdSystemAdmin && u.Status == UserStatus.Active);

            if (!admins.Any())
            {
                return;
            }

            var requester = transaction.ToUserId.HasValue
                ? await _userRepository.GetByIdAsync(transaction.ToUserId.Value)
                : null;
            var requesterName = requester?.FullName ?? requester?.UserName ?? "Người dùng";
            var content =
                $"{requesterName} vừa gửi yêu cầu rút {transaction.Amount:N0} VND. Vui lòng duyệt tại trang Duyệt rút tiền.";

            foreach (var admin in admins)
            {
                var notif = new Notification
                {
                    UserId = admin.Id,
                    Content = content,
                    Type = "Wallet_Withdrawal_Admin_Pending",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync();

                await _notificationPublisher.PublishNotificationPayloadAsync(admin.Id, new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Yêu cầu rút tiền mới",
                    Message = content,
                    Link = "/admin/withdraw-approval",
                    Type = "Wallet_Withdrawal_Admin_Pending",
                    CreateAt = notif.CreateAt
                });
            }
        }

        /// <summary>
        /// Tìm giao dịch nạp tiền theo mã tham chiếu — không ném exception, trả null nếu không tìm thấy.
        /// Dùng cho IPN để kiểm tra trước khi confirm.
        /// </summary>
        public async Task<Transaction?> GetDepositByReferenceCodeAsync(string referenceCode)
        {
            var transactions = await _transactionRepository.FindAsync(
                t => t.ReferenceCode == referenceCode
                    && (t.Type == "Deposit" || t.Type == "Platform_TopUp"));
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

            await PublishWalletNotificationAsync(
                userId,
                "Wallet_Withdrawal_Pending",
                "Yêu cầu rút tiền đã gửi",
                $"Yêu cầu rút {amount:N0} VND đã được gửi. Mã GD: {referenceCode}. Đang chờ Admin duyệt.");

            await NotifyAdminsOfPendingWithdrawalAsync(transaction);

            await _notificationPublisher.PublishWalletUpdatedAsync(userId, new WalletUpdatedPayload
            {
                WalletId = wallet.Id,
                SetupFundBalance = wallet.SetupFundBalance,
                WithdrawableBalance = wallet.WithdrawableBalance
            });

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

            if (isApproved)
            {
                await PublishWalletNotificationAsync(
                    wallet.UserId!.Value,
                    "Wallet_Withdrawal_Approve",
                    "Yêu cầu rút tiền được duyệt",
                    $"Yêu cầu rút {transaction.Amount:N0} VND (mã {transaction.ReferenceCode}) đã được Admin phê duyệt.");
            }
            else
            {
                var rejectNote = string.IsNullOrWhiteSpace(adminNote) ? string.Empty : $" Lý do: {adminNote}";
                await PublishWalletNotificationAsync(
                    wallet.UserId!.Value,
                    "Wallet_Withdrawal_Reject",
                    "Yêu cầu rút tiền bị từ chối",
                    $"Yêu cầu rút {transaction.Amount:N0} VND (mã {transaction.ReferenceCode}) bị từ chối.{rejectNote} Số dư đã được hoàn lại.");
            }

            await _notificationPublisher.PublishWalletUpdatedAsync(wallet.UserId!.Value, new WalletUpdatedPayload
            {
                WalletId = wallet.Id,
                SetupFundBalance = wallet.SetupFundBalance,
                WithdrawableBalance = wallet.WithdrawableBalance
            });

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

            await _notificationPublisher.PublishWalletUpdatedAsync(userId, new WalletUpdatedPayload
            {
                WalletId = wallet.Id,
                SetupFundBalance = wallet.SetupFundBalance,
                WithdrawableBalance = wallet.WithdrawableBalance
            });
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

            // Phát sự kiện cập nhật ví cho tác giả
            await _notificationPublisher.PublishWalletUpdatedAsync(task.MangakaId, new WalletUpdatedPayload
            {
                WalletId = wallet.Id,
                SetupFundBalance = wallet.SetupFundBalance,
                WithdrawableBalance = wallet.WithdrawableBalance
            });

            // Phát sự kiện cập nhật ví cho trợ lý nếu là luồng giải ngân thành công
            if (isApproved && task.AssistantId.HasValue)
            {
                var assistantWallet = await _walletRepository.GetWalletByUserIdAsync(task.AssistantId.Value);
                if (assistantWallet != null)
                {
                    await _notificationPublisher.PublishWalletUpdatedAsync(task.AssistantId.Value, new WalletUpdatedPayload
                    {
                        WalletId = assistantWallet.Id,
                        SetupFundBalance = assistantWallet.SetupFundBalance,
                        WithdrawableBalance = assistantWallet.WithdrawableBalance
                    });
                }
            }
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

        public async System.Threading.Tasks.Task ResolveDisputeAsync(int taskId, decimal assistantRate, int editorId)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ vẽ tranh không tồn tại.");
            }

            if (task.Status == "Approved" || task.Status == "Cancelled")
            {
                throw new BadRequestException("Nhiệm vụ vẽ tranh này đã hoàn thành hoặc đã bị hủy, không thể giải quyết tranh chấp.");
            }

            if (assistantRate < 0 || assistantRate > 1)
            {
                throw new BadRequestException("Tỷ lệ phân chia cho trợ lý phải nằm trong khoảng [0, 1].");
            }

            // Gọi ReleaseFundsAsync để phân chia tiền ký quỹ
            await ReleaseFundsAsync(taskId, isApproved: true, customPercentageForAssistant: assistantRate * 100m);

            // Cập nhật trạng thái nhiệm vụ
            task.Status = assistantRate > 0 ? "Approved" : "Cancelled";
            _tasksRepository.Update(task);

            // Ghi Dispute Log
            var disputeLog = new DisputeLog
            {
                EditorId = editorId,
                TaskId = taskId,
                EditorComment = $"Editor quyết định phân xử tranh chấp: Trợ lý nhận {assistantRate * 100}%, Tác giả nhận {(1 - assistantRate) * 100}%.",
                ResolutionType = "Arbitration",
                AssistantPercentage = assistantRate * 100m,
                MangakaPercentage = (1 - assistantRate) * 100m,
                ResolvedAt = DateTime.UtcNow
            };
            await _disputeLogRepository.AddAsync(disputeLog);
            await _unitOfWork.SaveChangesAsync();

            // Gửi thông báo realtime cho Mangaka và Assistant
            var taskStatusChanged = new TaskStatusChangedPayload
            {
                TaskId = task.Id,
                Status = task.Status,
                Message = $"Tranh chấp nhiệm vụ vẽ tranh đã được giải quyết bởi Editor. Trợ lý nhận {assistantRate * 100}% thù lao."
            };

            await _notificationPublisher.PublishTaskStatusChangedAsync(task.MangakaId, taskStatusChanged);
            if (task.AssistantId.HasValue)
            {
                await _notificationPublisher.PublishTaskStatusChangedAsync(task.AssistantId.Value, taskStatusChanged);
            }
        }

        public async Task<IEnumerable<DisputeListItemDto>> GetDisputesAsync(string? status)
        {
            // Lấy tất cả tasks có trạng thái Disputed hoặc có DisputeLog
            var disputedTasks = await _tasksRepository.FindAsync(t => t.Status == "Disputed");
            var disputeLogs = await _disputeLogRepository.GetAllAsync();
            var resolvedTaskIds = disputeLogs.Select(dl => dl.TaskId).Distinct().ToHashSet();

            // Merge: cả task đang disputed và task đã có dispute log (đã resolved)
            var allResolvedTasks = await _tasksRepository.FindAsync(t => resolvedTaskIds.Contains(t.Id));
            var allDisputeTasks = disputedTasks.Concat(allResolvedTasks).DistinctBy(t => t.Id).ToList();

            // Lọc theo status nếu có
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("Open", StringComparison.OrdinalIgnoreCase))
                {
                    allDisputeTasks = allDisputeTasks.Where(t => t.Status == "Disputed").ToList();
                }
                else if (status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
                {
                    allDisputeTasks = allDisputeTasks.Where(t => resolvedTaskIds.Contains(t.Id)).ToList();
                }
            }

            var result = new List<DisputeListItemDto>();
            foreach (var task in allDisputeTasks)
            {
                var mangaka = await _userRepository.GetByIdAsync(task.MangakaId);
                var assistant = task.AssistantId.HasValue ? await _userRepository.GetByIdAsync(task.AssistantId.Value) : null;
                var taskDisputeLog = disputeLogs.Where(dl => dl.TaskId == task.Id).OrderByDescending(dl => dl.ResolvedAt).FirstOrDefault();

                var disputeStatus = task.Status == "Disputed" ? "Open" : (taskDisputeLog != null ? "Resolved" : "Closed");

                result.Add(new DisputeListItemDto
                {
                    Id = task.Id,
                    TaskId = task.Id,
                    TaskTitle = task.Description,
                    MangakaName = mangaka?.FullName,
                    AssistantName = assistant?.FullName,
                    LockedAmount = task.PaymentAmount,
                    Status = disputeStatus,
                    CreatedAt = task.CreateAt,
                    ResolvedAt = taskDisputeLog?.ResolvedAt,
                    Resolution = taskDisputeLog?.EditorComment
                });
            }

            return result;
        }

        public async Task<DisputeDetailDto> GetDisputeDetailAsync(int taskId)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            var mangaka = await _userRepository.GetByIdAsync(task.MangakaId);
            var assistant = task.AssistantId.HasValue ? await _userRepository.GetByIdAsync(task.AssistantId.Value) : null;

            var disputeLogs = await _disputeLogRepository.FindAsync(dl => dl.TaskId == taskId);
            var latestLog = disputeLogs.OrderByDescending(dl => dl.ResolvedAt).FirstOrDefault();
            var disputeStatus = task.Status == "Disputed" ? "Open" : (latestLog != null ? "Resolved" : "Closed");

            // Lấy region info
            var region = await _regionRepository.GetByIdAsync(task.RegionId);

            // Lấy bằng chứng từ TaskVersions (ảnh nộp của Assistant)
            var versions = await _taskVersionRepository.FindAsync(v => v.TaskId == taskId);
            var evidence = new List<DisputeEvidenceDto>();

            foreach (var ver in versions.OrderBy(v => v.VersionNumber))
            {
                evidence.Add(new DisputeEvidenceDto
                {
                    SubmittedBy = "Assistant",
                    SubmitterName = assistant?.FullName,
                    Type = "Image",
                    Content = ver.SubmittedFileUrl,
                    CreatedAt = ver.SubmittedAt
                });

                // Lấy annotations trên version này (từ Mangaka)
                var annotations = await _annotationRepository.FindAsync(a => a.TaskVersionId == ver.Id);
                foreach (var ann in annotations)
                {
                    evidence.Add(new DisputeEvidenceDto
                    {
                        SubmittedBy = "Mangaka",
                        SubmitterName = mangaka?.FullName,
                        Type = "Annotation",
                        Content = ann.Comment,
                        CreatedAt = ann.CreateAt
                    });
                }
            }

            // Lấy submitted time từ version mới nhất
            var latestVersion = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

            return new DisputeDetailDto
            {
                Id = task.Id,
                TaskId = task.Id,
                TaskTitle = task.Description,
                MangakaName = mangaka?.FullName,
                AssistantName = assistant?.FullName,
                LockedAmount = task.PaymentAmount,
                Status = disputeStatus,
                CreatedAt = task.CreateAt,
                ResolvedAt = latestLog?.ResolvedAt,
                Resolution = latestLog?.EditorComment,
                TaskDeadline = task.Deadline,
                TaskSubmittedAt = latestVersion?.SubmittedAt,
                RegionInfo = region != null ? $"Page {region.PageId} - Region {region.Id}" : null,
                MangakaReason = task.FeedbackComment,
                AssistantReason = latestVersion != null ? $"Đã nộp phiên bản {latestVersion.VersionNumber}" : null,
                Evidence = evidence
            };
        }

        public async Task<ReconciliationReportDto> ReconcileTransactionsAsync(List<ReconciliationRow> rows)
        {
            var report = new ReconciliationReportDto
            {
                TotalRows = rows.Count
            };

            foreach (var row in rows)
            {
                var txs = await _transactionRepository.FindAsync(t => t.ReferenceCode == row.TxnRef);
                var tx = txs.FirstOrDefault();

                if (tx == null)
                {
                    report.UnresolvedCount++;
                    report.Details.Add($"Mã giao dịch {row.TxnRef}: Không tìm thấy trên hệ thống.");
                    continue;
                }

                if (tx.Amount != row.Amount)
                {
                    report.UnresolvedCount++;
                    report.Details.Add($"Mã giao dịch {row.TxnRef}: Số tiền không khớp (Hệ thống: {tx.Amount:N0}, VNPay: {row.Amount:N0}).");
                    continue;
                }

                if (tx.Status == "Success")
                {
                    report.MatchedCount++;
                    report.Details.Add($"Mã giao dịch {row.TxnRef}: Khớp (Success).");
                }
                else if (tx.Status == "Pending")
                {
                    if (row.ResponseCode == "00")
                    {
                        tx.Status = "Success";
                        _transactionRepository.Update(tx);

                        var wallet = await _walletRepository.GetByIdAsync(tx.WalletId);
                        if (wallet != null)
                        {
                            wallet.WithdrawableBalance += tx.Amount;
                            _walletRepository.Update(wallet);
                        }

                        report.ResolvedCount++;
                        report.Details.Add($"Mã giao dịch {row.TxnRef}: Đã xử lý (Cập nhật từ Pending sang Success).");

                        await _unitOfWork.SaveChangesAsync();

                        if (wallet != null && wallet.UserId.HasValue)
                        {
                            await _notificationPublisher.PublishWalletUpdatedAsync(wallet.UserId.Value, new WalletUpdatedPayload
                            {
                                WalletId = wallet.Id,
                                SetupFundBalance = wallet.SetupFundBalance,
                                WithdrawableBalance = wallet.WithdrawableBalance
                            });
                        }
                    }
                    else
                    {
                        tx.Status = "Failed";
                        _transactionRepository.Update(tx);
                        await _unitOfWork.SaveChangesAsync();

                        report.UnresolvedCount++;
                        report.Details.Add($"Mã giao dịch {row.TxnRef}: VNPay báo lỗi (ResponseCode: {row.ResponseCode}). Cập nhật trạng thái thành Failed.");
                    }
                }
                else
                {
                    report.UnresolvedCount++;
                    report.Details.Add($"Mã giao dịch {row.TxnRef}: Trạng thái không hợp lệ để đối soát (Trạng thái hiện tại: {tx.Status}).");
                }
            }

            return report;
        }
    }
}
