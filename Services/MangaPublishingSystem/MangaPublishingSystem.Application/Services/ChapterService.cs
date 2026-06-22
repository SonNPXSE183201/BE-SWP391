using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs.Reviews;
using MangaPublishingSystem.Application.DTOs.Notifications;

namespace MangaPublishingSystem.Application.Services
{
    public class ChapterService : GenericService<Chapter>, IChapterService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;

        public ChapterService(
            IChapterRepository repository, 
            IUnitOfWork unitOfWork,
            ISeriesRepository seriesRepository,
            IContractRepository contractRepository,
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher) 
            : base(repository, unitOfWork)
        {
            _chapterRepository = repository;
            _seriesRepository = seriesRepository;
            _contractRepository = contractRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
        }

        public async Task<IEnumerable<Chapter>> GetChaptersBySeriesIdAsync(int seriesId)
        {
            return await _repository.FindAsync(c => c.SeriesId == seriesId);
        }

        public async System.Threading.Tasks.Task ApproveChapterAsync(int chapterId, int editorId, ApproveChapterDto dto)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }
            if (chapter.Status == "Approved")
            {
                throw new BadRequestException("Chapter đã được duyệt trước đó.");
            }

            var contracts = await _contractRepository.FindAsync(c => c.SeriesId == chapter.SeriesId && c.Status == "Signed");
            var contract = contracts.FirstOrDefault();
            if (contract == null)
            {
                throw new BadRequestException("Không tìm thấy hợp đồng hợp lệ (Signed) cho bộ truyện này.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                chapter.Status = "Approved";
                chapter.ValidPageCount = dto.ValidPageCount;
                chapter.AppliedGenkouryoPrice = contract.BaseGenkouryoPrice;
                chapter.QcChecklistData = dto.QcChecklistData;
                _repository.Update(chapter);

                decimal amount = dto.ValidPageCount * contract.BaseGenkouryoPrice;

                var wallet = await _walletRepository.GetWalletByUserIdAsync(contract.UserId);
                if (wallet == null)
                {
                    throw new NotFoundException("Không tìm thấy ví của tác giả.");
                }
                wallet.WithdrawableBalance += amount;
                _walletRepository.Update(wallet);

                var transaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = "Genkouryo_Payment",
                    ReferenceId = chapterId,
                    Amount = amount,
                    WithdrawableAmount = amount,
                    Status = "Success",
                    ToUserId = contract.UserId
                };
                await _transactionRepository.AddAsync(transaction);

                var notif = new Notification
                {
                    UserId = contract.UserId,
                    Content = $"Chapter {chapter.ChapterNumber}: {chapter.Title} đã được duyệt. Nhuận bút giải ngân: {amount:N0} VND.",
                    Type = "Genkouryo_Paid",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var notifPayload = new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Duyệt Chapter & Giải ngân",
                    Message = notif.Content,
                    Link = $"/chapters/{chapterId}",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                };
                await _notificationPublisher.PublishNotificationPayloadAsync(contract.UserId, notifPayload);

                await _notificationPublisher.PublishWalletUpdatedAsync(contract.UserId, new WalletUpdatedPayload
                {
                    WalletId = wallet.Id,
                    SetupFundBalance = wallet.SetupFundBalance,
                    WithdrawableBalance = wallet.WithdrawableBalance
                });
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async System.Threading.Tasks.Task RejectChapterAsync(int chapterId, int editorId, RejectChapterDto dto)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }
            if (chapter.Status == "Approved")
            {
                throw new BadRequestException("Chapter đã được duyệt trước đó, không thể từ chối.");
            }

            var series = await _seriesRepository.GetByIdAsync(chapter.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện liên kết với chapter này.");
            }

            chapter.Status = "Rejected";
            _repository.Update(chapter);
            await _unitOfWork.SaveChangesAsync();

            var notif = new Notification
            {
                UserId = series.MangakaId,
                Content = $"Chapter {chapter.ChapterNumber}: {chapter.Title} bị từ chối duyệt. Lý do: {dto.FeedbackComment}",
                Type = "Chapter_Rejected",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync();

            var notifPayload = new NotificationPayload
            {
                Id = notif.Id,
                Title = "Chapter bị từ chối duyệt",
                Message = notif.Content,
                Link = $"/chapters/{chapterId}",
                Type = notif.Type,
                CreateAt = notif.CreateAt
            };
            await _notificationPublisher.PublishNotificationPayloadAsync(series.MangakaId, notifPayload);
        }

        public async Task<IEnumerable<Chapter>> GetPendingReviewChaptersForEditorAsync(int editorId)
        {
            return await _chapterRepository.GetPendingReviewChaptersWithDetailsAsync(editorId);
        }

        public async System.Threading.Tasks.Task UpdateDeadlineAsync(int chapterId, DateTime deadline)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }
            chapter.SubmissionDeadline = deadline;
            _repository.Update(chapter);
            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task PublishChapterAsync(int chapterId)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }
            if (chapter.Status != "Approved")
            {
                throw new BadRequestException("Chỉ những chapter đã được phê duyệt (Approved) mới có thể xuất bản.");
            }
            chapter.Status = "Published";
            _repository.Update(chapter);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}