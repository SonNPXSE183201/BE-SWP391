using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.Services.Admin
{
    public class AdminContractService : IAdminContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationPublisher _notificationPublisher;

        public AdminContractService(
            IContractRepository contractRepository,
            ISeriesRepository seriesRepository,
            IUnitOfWork unitOfWork,
            INotificationPublisher notificationPublisher)
        {
            _contractRepository = contractRepository;
            _seriesRepository = seriesRepository;
            _unitOfWork = unitOfWork;
            _notificationPublisher = notificationPublisher;
        }

        public async Task<List<ApprovedSeriesContractDto>> GetApprovedSeriesAsync()
        {
            var seriesList = await _contractRepository.GetApprovedSeriesForContractsAsync();

            return seriesList.Select(s =>
            {
                var contract = s.Contracts.FirstOrDefault();
                return new ApprovedSeriesContractDto
                {
                    Id = s.Id.ToString(),
                    Title = s.Title,
                    MangakaName = s.Mangaka?.FullName ?? "N/A",
                    ApprovedAt = (s.UpdateAt ?? s.CreateAt).ToUniversalTime().ToString("o"),
                    ApprovedBudget = s.ApprovedProductionBudget > 0 ? s.ApprovedProductionBudget : s.EstimatedProductionBudget,
                    PublishSchedule = string.IsNullOrWhiteSpace(s.PublicationSchedule) ? "Chưa thiết lập" : s.PublicationSchedule,
                    HasContract = contract != null,
                    ContractId = contract?.Id.ToString(),
                    GenkouryoPrice = contract?.BaseGenkouryoPrice,
                    SignedDate = contract?.SignedDate?.ToUniversalTime().ToString("o"),
                    ContractStatus = contract?.Status,
                    Addendums = contract?.ContractAddendums?.OrderByDescending(a => a.EffectiveDate).Select(a => new ContractAddendumDto
                    {
                        Id = a.Id.ToString(),
                        NewGenkouryoPrice = a.NewGenkouryoPrice,
                        EffectiveDate = a.EffectiveDate.ToUniversalTime().ToString("o"),
                        SignedDate = a.SignedDate?.ToUniversalTime().ToString("o")
                    }).ToList(),
                    Genres = ParseGenres(s.Genre)
                };
            }).ToList();
        }

        public async Task<CreateContractResponseDto> CreateContractAsync(CreateContractRequestDto dto)
        {
            var seriesId = int.Parse(dto.SeriesId);

            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (series.Status != "Fund_Pending")
            {
                throw new BadRequestException("Mangaka chua xac nhan muc von cho bo truyen nay.");
            }

            var existing = await _contractRepository.GetBySeriesIdAsync(seriesId);
            if (existing != null)
            {
                throw new ConflictException("Bộ truyện này đã có hợp đồng.");
            }

            var contract = new Contract
            {
                UserId = series.MangakaId,
                SeriesId = seriesId,
                BaseGenkouryoPrice = dto.BaseGenkouryoPrice,
                Status = "Pending",
                SignedDate = null
            };

            await _contractRepository.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();
            await _notificationPublisher.PublishNotificationPayloadAsync(series.MangakaId, new NotificationPayload
            {
                Title = "Hop dong da duoc lap",
                Message = $"Admin da lap hop dong cho bo truyen '{series.Title}'. Vui long xem chi tiet va xac nhan ky ket.",
                Type = "Contract_Created",
                Link = $"/mangaka/series/{series.Id}",
                CreateAt = DateTime.UtcNow
            });
            await _notificationPublisher.PublishBoardDataChangedAsync();

            return new CreateContractResponseDto
            {
                ContractId = contract.Id.ToString(),
                SeriesId = seriesId.ToString(),
                BaseGenkouryoPrice = contract.BaseGenkouryoPrice
            };
        }

        public async System.Threading.Tasks.Task UpdateContractAsync(int contractId, UpdateContractRequestDto dto)
        {
            var contract = await _contractRepository.GetWithSeriesAsync(contractId);
            if (contract == null)
            {
                throw new NotFoundException("Không tìm thấy hợp đồng.");
            }

            if (!dto.GenkouryoPrice.HasValue)
            {
                throw new BadRequestException("Phải cung cấp đơn giá nhuận bút mới khi cập nhật phụ lục hợp đồng.");
            }

            var effectiveDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.EndDate) && DateTime.TryParse(dto.EndDate, out var parsedEffectiveDate))
            {
                effectiveDate = parsedEffectiveDate.ToUniversalTime();
            }

            var addendum = new ContractAddendum
            {
                ContractId = contractId,
                NewGenkouryoPrice = dto.GenkouryoPrice.Value,
                EffectiveDate = effectiveDate,
                SignedDate = DateTime.UtcNow
            };

            await _contractRepository.AddAddendumAsync(addendum);
            await _unitOfWork.SaveChangesAsync();
        }

        private static List<string> ParseGenres(string? genre)
        {
            if (string.IsNullOrWhiteSpace(genre))
            {
                return new List<string>();
            }

            return genre.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
    }
}
