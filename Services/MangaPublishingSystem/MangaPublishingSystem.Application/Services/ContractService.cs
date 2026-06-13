using System;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Contract;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class ContractService : GenericService<Contract>, IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IWalletService _walletService;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;

        public ContractService(
            IContractRepository repository, 
            IUnitOfWork unitOfWork,
            ISeriesRepository seriesRepository,
            IWalletService walletService,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher) 
            : base(repository, unitOfWork)
        {
            _contractRepository = repository;
            _seriesRepository = seriesRepository;
            _walletService = walletService;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
        }

        public async Task<ContractDto> CreateContractAsync(CreateContractDto dto)
        {
            var series = await _seriesRepository.GetByIdAsync(dto.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "Board_Approved")
            {
                throw new ConflictException("Chỉ có thể tạo hợp đồng cho bộ truyện đã được Hội đồng biên tập phê duyệt.");
            }

            // Check if there is an active or pending contract for this series
            var existingContracts = await _contractRepository.FindAsync(c => c.SeriesId == dto.SeriesId && (c.Status == "Pending" || c.Status == "Active"));
            if (existingContracts.Any())
            {
                throw new ConflictException("Đã tồn tại hợp đồng đang chờ ký hoặc đang hoạt động cho bộ truyện này.");
            }

            var contract = new Contract
            {
                UserId = dto.UserId,
                SeriesId = dto.SeriesId,
                BaseGenkouryoPrice = dto.BaseGenkouryoPrice,
                Status = "Pending"
            };

            await _contractRepository.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            // Send notification to Mangaka
            var notif = new Notification
            {
                UserId = dto.UserId,
                Content = $"Hợp đồng cho bộ truyện '{series.Title}' đã được chuẩn bị. Vui lòng kiểm tra và xác nhận nhận quỹ.",
                Type = "Contract_Created",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _notificationPublisher.PublishNotificationAsync(dto.UserId, notif.Content, notif.Type);
            await _unitOfWork.SaveChangesAsync();

            return contract.ToDto();
        }

        public async System.Threading.Tasks.Task AcceptContractAsync(int contractId, int mangakaId)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new NotFoundException("Hợp đồng không tồn tại.");
            }

            if (contract.Status != "Pending")
            {
                throw new ConflictException("Hợp đồng này không ở trạng thái chờ ký.");
            }

            if (contract.UserId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền ký hợp đồng này.");
            }

            var series = await _seriesRepository.GetByIdAsync(contract.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện liên kết với hợp đồng không tồn tại.");
            }

            // Update Contract status
            contract.Status = "Active";
            contract.SignedDate = DateTime.UtcNow;
            _contractRepository.Update(contract);

            // Update Series status
            series.Status = "In_Production";
            _seriesRepository.Update(series);

            // Fund the Mangaka's SetupFundBalance wallet
            await _walletService.FundWalletAsync(mangakaId, series.ApprovedProductionBudget, series.Id);

            // Add notification
            var notif = new Notification
            {
                UserId = mangakaId,
                Content = $"Hợp đồng bộ truyện '{series.Title}' ký kết thành công. Ngân sách {series.ApprovedProductionBudget:N0} VND đã được nạp vào ví tài trợ của bạn.",
                Type = "Contract_Accepted",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _notificationPublisher.PublishNotificationAsync(mangakaId, notif.Content, notif.Type);

            // Send notification to Editor if assigned
            if (series.EditorId.HasValue)
            {
                var notifEditor = new Notification
                {
                    UserId = series.EditorId.Value,
                    Content = $"Tác giả đã chấp nhận hợp đồng cho bộ truyện '{series.Title}'. Bộ truyện bắt đầu đưa vào sản xuất.",
                    Type = "Contract_Accepted_Editor",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notifEditor);
                await _notificationPublisher.PublishNotificationAsync(series.EditorId.Value, notifEditor.Content, notifEditor.Type);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeclineContractAsync(int contractId, int mangakaId, string declineReason)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new NotFoundException("Hợp đồng không tồn tại.");
            }

            if (contract.Status != "Pending")
            {
                throw new ConflictException("Hợp đồng này không ở trạng thái chờ ký.");
            }

            if (contract.UserId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền từ chối hợp đồng này.");
            }

            var series = await _seriesRepository.GetByIdAsync(contract.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện liên kết với hợp đồng không tồn tại.");
            }

            // Update Contract status
            contract.Status = "Declined";
            _contractRepository.Update(contract);

            // Reset Series status to Draft so Mangaka can modify details and resubmit
            series.Status = "Draft";
            _seriesRepository.Update(series);

            // Add notification to Mangaka
            var notif = new Notification
            {
                UserId = mangakaId,
                Content = $"Bạn đã từ chối hợp đồng bộ truyện '{series.Title}'. Trạng thái truyện được đưa về Draft.",
                Type = "Contract_Declined",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _notificationPublisher.PublishNotificationAsync(mangakaId, notif.Content, notif.Type);

            // Send notification to Editor if assigned
            if (series.EditorId.HasValue)
            {
                var notifEditor = new Notification
                {
                    UserId = series.EditorId.Value,
                    Content = $"Tác giả đã từ chối hợp đồng cho bộ truyện '{series.Title}' với lý do: {declineReason}.",
                    Type = "Contract_Declined_Editor",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notifEditor);
                await _notificationPublisher.PublishNotificationAsync(series.EditorId.Value, notifEditor.Content, notifEditor.Type);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}