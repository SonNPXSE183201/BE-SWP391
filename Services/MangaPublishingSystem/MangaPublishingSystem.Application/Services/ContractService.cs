using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Contracts;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Notifications;
using SelectPdf;

namespace MangaPublishingSystem.Application.Services
{
    public class ContractService : GenericService<Contract>, IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly INotificationPublisher _notificationPublisher;

        private readonly IContractTemplateRepository _templateRepository;
        private readonly IPlatformWalletService _platformWalletService;
        private readonly IWalletRepository _walletRepository;
        private readonly IStorageService _storageService;

        public ContractService(
            IContractRepository repository,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            ISeriesRepository seriesRepository,
            IContractTemplateRepository templateRepository,
            INotificationPublisher notificationPublisher,
            IPlatformWalletService platformWalletService,
            IWalletRepository walletRepository,
            IStorageService storageService) : base(repository, unitOfWork)
        {
            _contractRepository = repository;
            _userRepository = userRepository;
            _seriesRepository = seriesRepository;
            _templateRepository = templateRepository;
            _notificationPublisher = notificationPublisher;
            _platformWalletService = platformWalletService;
            _walletRepository = walletRepository;
            _storageService = storageService;
        }

        public async Task<ContractDto> GenerateContractAsync(CreateContractDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null || user.RoleId != 4)
            {
                throw new NotFoundException("Không tìm thấy tác giả Mangaka.");
            }

            if (string.IsNullOrWhiteSpace(user.CitizenId) || user.CitizenIdIssueDate == null || string.IsNullOrWhiteSpace(user.CitizenIdIssuePlace))
            {
                throw new BadRequestException("Tác giả chưa cập nhật đầy đủ thông tin CMND/CCCD, không thể tạo hợp đồng.");
            }

            var series = await _seriesRepository.GetByIdAsync(dto.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.MangakaId != dto.UserId)
            {
                throw new BadRequestException("Bộ truyện không thuộc về tác giả này.");
            }

            if (series.Status != "Fund_Pending" && series.Status != "Approved")
            {
                throw new BadRequestException("Hợp đồng chỉ có thể được tạo cho Series đã được hội đồng duyệt (Fund_Pending).");
            }

            if (dto.BaseGenkouryoPrice <= 0)
            {
                throw new BadRequestException("Đơn giá nhuận bút trang phải lớn hơn 0.");
            }

            var existingActive = await _contractRepository.FindAsync(c => c.SeriesId == dto.SeriesId && (c.Status == "Active" || c.Status == "Pending"));
            if (existingActive.Any())
            {
                throw new ConflictException("Bộ truyện này đã có một hợp đồng đang hoạt động hoặc đang chờ ký.");
            }

            var template = dto.TemplateId > 0 
                ? await _templateRepository.GetByIdAsync(dto.TemplateId)
                : await _templateRepository.GetActiveTemplateAsync();

            if (template == null)
            {
                throw new NotFoundException("Không tìm thấy mẫu hợp đồng khả dụng.");
            }

            var contract = new Contract
            {
                UserId = dto.UserId,
                SeriesId = dto.SeriesId,
                TemplateId = template.Id,
                BaseGenkouryoPrice = dto.BaseGenkouryoPrice,
                Status = "Pending",
                ExpirationDate = DateTime.Now.AddDays(7) // Hạn ký là 7 ngày
            };

            // 1. Chuẩn bị nội dung HTML từ Template
            var htmlContent = template.Content
                .Replace("{{MangakaFullName}}", !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.UserName)
                .Replace("{{MangakaName}}", !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.UserName) // fallback for older templates
                .Replace("{{MangakaPenName}}", user.PenName ?? "")
                .Replace("{{MangakaCitizenId}}", user.CitizenId ?? "")
                .Replace("{{MangakaCitizenIdIssueDate}}", user.CitizenIdIssueDate.HasValue ? user.CitizenIdIssueDate.Value.ToString("dd/MM/yyyy") : "")
                .Replace("{{MangakaCitizenIdIssuePlace}}", user.CitizenIdIssuePlace ?? "")
                .Replace("{{MangakaEmail}}", user.Email ?? "")
                .Replace("{{MangakaPhoneNumber}}", user.PhoneNumber ?? "")
                .Replace("{{SeriesTitle}}", series.Title)
                .Replace("{{SeriesGenre}}", series.Genre ?? "")
                .Replace("{{BaseGenkouryoPrice}}", dto.BaseGenkouryoPrice.ToString("N0"))
                .Replace("{{BasePrice}}", dto.BaseGenkouryoPrice.ToString("N0") + " VND") // fallback
                .Replace("{{ExpirationDate}}", contract.ExpirationDate.Value.ToString("dd/MM/yyyy"))
                .Replace("{{Date}}", DateTime.Now.ToString("dd/MM/yyyy")) // fallback
                .Replace("{{Day}}", DateTime.Now.ToString("dd"))
                .Replace("{{Month}}", DateTime.Now.ToString("MM"))
                .Replace("{{Year}}", DateTime.Now.ToString("yyyy"))
                .Replace("{{SeriesId}}", series.Id.ToString("D4"))
                .Replace("{{ContractDurationMonths}}", "12")
                .Replace("{{ApprovedProductionBudget}}", series.ApprovedProductionBudget.ToString("N0"))
                .Replace("{{PlatformName}}", "Công ty TNHH Manga Publishing System")
                .Replace("{{PlatformRepresentativeName}}", "Nguyễn Văn A")
                .Replace("{{PlatformRepresentativeRole}}", "Giám đốc")
                .Replace("{{PlatformAddress}}", "FPT University")
                .Replace("{{PlatformEmail}}", "contact@mangapublishing.vn");

            // 2. Tạo file PDF từ HTML
            var converter = new HtmlToPdf();
            var pdfDocument = converter.ConvertHtmlString(htmlContent);
            using var pdfStream = new MemoryStream();
            pdfDocument.Save(pdfStream);
            pdfDocument.Close();
            pdfStream.Position = 0;

            // 3. Upload file PDF lên Storage
            string fileName = $"contract_{series.Id}_{DateTime.Now.Ticks}.pdf";
            contract.ContractFileUrl = await _storageService.UploadFileAsync(pdfStream, fileName, "application/pdf", "contracts");

            await _contractRepository.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            await _notificationPublisher.PublishNotificationPayloadAsync(dto.UserId, new NotificationPayload
            {
                Title = "Hợp đồng mới cần ký",
                Message = $"Admin đã tạo hợp đồng cho bộ truyện '{series.Title}'. Vui lòng xem và ký hợp đồng trước ngày {contract.ExpirationDate:dd/MM/yyyy}.",
                Type = "Contract_Pending",
                Link = $"/mangaka/contracts/{contract.Id}"
            });

            var created = await _contractRepository.GetContractWithDetailsAsync(contract.Id);
            return MapToDto(created!);
        }

        public async Task<ContractDto> SignContractAsync(int contractId)
        {
            var contract = await _contractRepository.GetContractWithDetailsAsync(contractId);
            if (contract == null) throw new NotFoundException("Hợp đồng không tồn tại.");

            if (contract.Status != "Pending")
            {
                throw new BadRequestException("Hợp đồng này không ở trạng thái chờ ký.");
            }

            if (contract.ExpirationDate.HasValue && DateTime.Now > contract.ExpirationDate.Value)
            {
                throw new BadRequestException("Hợp đồng này đã hết hạn ký.");
            }

            contract.Status = "Active";
            contract.SignedDate = DateTime.Now;

            // Cập nhật trạng thái Series thành In Production khi ký hợp đồng
            if (contract.Series != null)
            {
                contract.Series.Status = "In Production";
                _seriesRepository.Update(contract.Series);
            }

            _contractRepository.Update(contract);

            // F5.4 Giải ngân quỹ sản xuất
            var mangakaWallet = await _walletRepository.GetWalletByUserIdAsync(contract.UserId);
            if (mangakaWallet != null && contract.Series != null)
            {
                // Gọi sang PlatformWalletService để disbuse (trừ ví platform, cộng SetupFund mangaka)
                await _platformWalletService.DisburseProductionFundAsync(
                    contract.SeriesId, 
                    contract.UserId, 
                    contract.Series.ApprovedProductionBudget > 0 ? contract.Series.ApprovedProductionBudget : contract.Series.EstimatedProductionBudget, 
                    mangakaWallet);
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToDto(contract);
        }

        public async Task<ContractDto> RejectContractAsync(int contractId)
        {
            var contract = await _contractRepository.GetContractWithDetailsAsync(contractId);
            if (contract == null) throw new NotFoundException("Hợp đồng không tồn tại.");

            if (contract.Status != "Pending")
            {
                throw new BadRequestException("Chỉ có thể từ chối hợp đồng đang chờ ký.");
            }

            contract.Status = "Rejected";
            _contractRepository.Update(contract);

            if (contract.Series != null)
            {
                contract.Series.ContractRejectionCount++;
                if (contract.Series.ContractRejectionCount >= 3)
                {
                    contract.Series.Status = "Cancelled";
                }
                else
                {
                    contract.Series.Status = "Draft"; // Quay lại từ đầu để Mangaka nộp duyệt lại
                }
                _seriesRepository.Update(contract.Series);
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToDto(contract);
        }

        public async Task<ContractDto> UpdateContractAsync(int id, UpdateContractDto dto)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                throw new NotFoundException("Hợp đồng không tồn tại.");
            }

            if (dto.BaseGenkouryoPrice < 0)
            {
                throw new BadRequestException("Đơn giá nhuận bút trang không được nhỏ hơn 0.");
            }

            if (dto.Status == "Active" && contract.Status != "Active")
            {
                var existingActive = await _contractRepository.FindAsync(c => c.SeriesId == contract.SeriesId && c.Status == "Active" && c.Id != contract.Id);
                if (existingActive.Any())
                {
                    throw new ConflictException("Bộ truyện này đã có một hợp đồng đang hoạt động.");
                }
            }

            contract.BaseGenkouryoPrice = dto.BaseGenkouryoPrice;
            contract.Status = dto.Status;
            
            _contractRepository.Update(contract);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _contractRepository.GetContractWithDetailsAsync(contract.Id);
            return MapToDto(updated!);
        }

        public async Task<ContractDto> GetContractByIdAsync(int id)
        {
            var contract = await _contractRepository.GetContractWithDetailsAsync(id);
            if (contract == null)
            {
                throw new NotFoundException("Hợp đồng không tồn tại.");
            }
            return MapToDto(contract);
        }

        public async Task<IEnumerable<ContractDto>> GetContractsAsync()
        {
            var contracts = await _contractRepository.GetContractsWithDetailsAsync();
            return contracts.Select(MapToDto);
        }

        public async System.Threading.Tasks.Task DeleteContractAsync(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                throw new NotFoundException("Hợp đồng không tồn tại.");
            }
            _contractRepository.Delete(contract);
            await _unitOfWork.SaveChangesAsync();
        }

        private static ContractDto MapToDto(Contract contract)
        {
            return new ContractDto
            {
                Id = contract.Id,
                UserId = contract.UserId,
                MangakaName = contract.User?.FullName,
                SeriesId = contract.SeriesId,
                SeriesTitle = contract.Series?.Title,
                TemplateId = contract.TemplateId,
                ContractFileUrl = contract.ContractFileUrl,
                ExpirationDate = contract.ExpirationDate,
                BaseGenkouryoPrice = contract.BaseGenkouryoPrice,
                SignedDate = contract.SignedDate,
                Status = contract.Status,
                CreateAt = contract.CreateAt
            };
        }
    }
}