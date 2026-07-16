using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.DTOs.Contracts;
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
        private readonly IContractService _contractService;

        public AdminContractService(
            IContractRepository contractRepository,
            ISeriesRepository seriesRepository,
            IUnitOfWork unitOfWork,
            INotificationPublisher notificationPublisher,
            IContractService contractService)
        {
            _contractRepository = contractRepository;
            _seriesRepository = seriesRepository;
            _unitOfWork = unitOfWork;
            _notificationPublisher = notificationPublisher;
            _contractService = contractService;
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
                    PublishSchedule = string.IsNullOrWhiteSpace(s.PublicationSchedule) ? "Not configured" : s.PublicationSchedule,
                    HasContract = contract != null,
                    ContractId = contract?.Id.ToString(),
                    GenkouryoPrice = contract?.BaseGenkouryoPrice,
                    SignedDate = contract?.SignedDate?.ToUniversalTime().ToString("o"),
                    ContractStatus = contract?.Status,
                    ContractFileUrl = contract?.ContractFileUrl,
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
                throw new NotFoundException("Series was not found.");
            }

            if (series.Status != "Fund_Pending" && series.Status != "Approved")
            {
                throw new BadRequestException("Series is not ready for contract generation.");
            }

            var contract = await _contractService.GenerateContractAsync(new CreateContractDto
            {
                UserId = series.MangakaId,
                SeriesId = seriesId,
                TemplateId = dto.TemplateId,
                BaseGenkouryoPrice = dto.BaseGenkouryoPrice
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
                throw new NotFoundException("Contract was not found.");
            }

            if (!dto.GenkouryoPrice.HasValue)
            {
                throw new BadRequestException("A new genkouryo price is required when creating a contract addendum.");
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
