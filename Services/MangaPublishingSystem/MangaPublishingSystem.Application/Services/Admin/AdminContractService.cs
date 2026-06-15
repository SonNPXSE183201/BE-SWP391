using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Admin;
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

        public AdminContractService(
            IContractRepository contractRepository,
            ISeriesRepository seriesRepository,
            IUnitOfWork unitOfWork)
        {
            _contractRepository = contractRepository;
            _seriesRepository = seriesRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ApprovedSeriesContractDto>> GetApprovedSeriesAsync()
        {
            var seriesList = await _contractRepository.GetApprovedSeriesForContractsAsync();

            return seriesList.Select(s => new ApprovedSeriesContractDto
            {
                Id = s.Id.ToString(),
                Title = s.Title,
                MangakaName = s.Mangaka?.FullName ?? "N/A",
                ApprovedAt = (s.UpdateAt ?? s.CreateAt).ToUniversalTime().ToString("o"),
                ApprovedBudget = s.ApprovedProductionBudget > 0 ? s.ApprovedProductionBudget : s.EstimatedProductionBudget,
                PublishSchedule = string.IsNullOrWhiteSpace(s.PublicationSchedule) ? "Chưa thiết lập" : s.PublicationSchedule,
                HasContract = s.Contracts.Any(),
                Genres = ParseGenres(s.Genre)
            }).ToList();
        }

        public async Task<bool> CreateContractAsync(CreateContractRequestDto dto)
        {
            if (!int.TryParse(dto.SeriesId, out var seriesId))
            {
                throw new BadRequestException("Mã series không hợp lệ.");
            }

            if (dto.BaseGenkouryoPrice <= 0)
            {
                throw new BadRequestException("Đơn giá nhuận bút phải lớn hơn 0.");
            }

            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (series.Status != "Board_Approved" && series.Status != "Approved")
            {
                throw new BadRequestException("Bộ truyện chưa được Hội đồng phê duyệt.");
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
                Status = "Active",
                SignedDate = DateTime.UtcNow
            };

            await _contractRepository.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async System.Threading.Tasks.Task UpdateContractAsync(int contractId, UpdateContractRequestDto dto)
        {
            var contract = await _contractRepository.GetWithSeriesAsync(contractId);
            if (contract == null)
            {
                throw new NotFoundException("Không tìm thấy hợp đồng.");
            }

            if (dto.GenkouryoPrice.HasValue)
            {
                if (dto.GenkouryoPrice.Value <= 0)
                {
                    throw new BadRequestException("Đơn giá nhuận bút phải lớn hơn 0.");
                }

                contract.BaseGenkouryoPrice = dto.GenkouryoPrice.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.EndDate) && DateTime.TryParse(dto.EndDate, out var endDate))
            {
                contract.SignedDate = endDate.ToUniversalTime();
            }

            _contractRepository.Update(contract);
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
