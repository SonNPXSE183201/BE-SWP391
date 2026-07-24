using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Contracts;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Notifications;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

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

            var existingActive = await _contractRepository.FindAsync(c => c.SeriesId == dto.SeriesId && (c.Status == "Active" || c.Status == "Signed" || c.Status == "Pending"));
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
            var htmlContent = EnsurePublicationSchedulePlaceholder(template.Content)
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
                .Replace("{{PublicationSchedule}}", FormatPublicationSchedule(series.PublicationSchedule))
                .Replace("{{PublishSchedule}}", FormatPublicationSchedule(series.PublicationSchedule))
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

            htmlContent = ApplySignatureStatus(htmlContent, mangakaSigned: false);

            // 2. Tạo file PDF từ HTML (tự động chuyển sang PdfSharpCore fallback nếu chạy trên Linux Docker không có kernel32.dll)
            htmlContent = ApplyContractPrintStyles(htmlContent);
            var pdfBytes = GeneratePdfBytes(htmlContent);
            using var pdfStream = new MemoryStream(pdfBytes);

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

            if (contract.User != null && contract.Series != null && contract.TemplateId.HasValue)
            {
                var template = await _templateRepository.GetByIdAsync(contract.TemplateId.Value);
                if (template != null)
                {
                    contract.ContractFileUrl = await GenerateAndUploadContractPdfAsync(
                        contract.User,
                        contract.Series,
                        contract,
                        template,
                        mangakaSigned: true);
                }
            }

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
                var existingActive = await _contractRepository.FindAsync(c => c.SeriesId == contract.SeriesId && (c.Status == "Active" || c.Status == "Signed") && c.Id != contract.Id);
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
                PublicationSchedule = contract.Series?.PublicationSchedule,
                PublishSchedule = FormatPublicationSchedule(contract.Series?.PublicationSchedule),
                TemplateId = contract.TemplateId,
                ContractFileUrl = contract.ContractFileUrl,
                ExpirationDate = contract.ExpirationDate,
                BaseGenkouryoPrice = contract.BaseGenkouryoPrice,
                SignedDate = contract.SignedDate,
                Status = contract.Status,
                CreateAt = contract.CreateAt
            };
        }

        private async Task<string> GenerateAndUploadContractPdfAsync(
            User user,
            Series series,
            Contract contract,
            ContractTemplate template,
            bool mangakaSigned)
        {
            var htmlContent = EnsurePublicationSchedulePlaceholder(template.Content)
                .Replace("{{MangakaFullName}}", !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.UserName)
                .Replace("{{MangakaName}}", !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.UserName)
                .Replace("{{MangakaPenName}}", user.PenName ?? "")
                .Replace("{{MangakaCitizenId}}", user.CitizenId ?? "")
                .Replace("{{MangakaCitizenIdIssueDate}}", user.CitizenIdIssueDate.HasValue ? user.CitizenIdIssueDate.Value.ToString("dd/MM/yyyy") : "")
                .Replace("{{MangakaCitizenIdIssuePlace}}", user.CitizenIdIssuePlace ?? "")
                .Replace("{{MangakaEmail}}", user.Email ?? "")
                .Replace("{{MangakaPhoneNumber}}", user.PhoneNumber ?? "")
                .Replace("{{SeriesTitle}}", series.Title)
                .Replace("{{SeriesGenre}}", series.Genre ?? "")
                .Replace("{{PublicationSchedule}}", FormatPublicationSchedule(series.PublicationSchedule))
                .Replace("{{PublishSchedule}}", FormatPublicationSchedule(series.PublicationSchedule))
                .Replace("{{BaseGenkouryoPrice}}", contract.BaseGenkouryoPrice.ToString("N0"))
                .Replace("{{BasePrice}}", contract.BaseGenkouryoPrice.ToString("N0") + " VND")
                .Replace("{{ExpirationDate}}", contract.ExpirationDate.HasValue ? contract.ExpirationDate.Value.ToString("dd/MM/yyyy") : "")
                .Replace("{{Date}}", DateTime.Now.ToString("dd/MM/yyyy"))
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

            htmlContent = ApplySignatureStatus(htmlContent, mangakaSigned);
            htmlContent = ApplyContractPrintStyles(htmlContent);

            var pdfBytes = GeneratePdfBytes(htmlContent);
            using var pdfStream = new MemoryStream(pdfBytes);

            var fileName = $"contract_{series.Id}_{DateTime.Now.Ticks}.pdf";
            return await _storageService.UploadFileAsync(pdfStream, fileName, "application/pdf", "contracts");
        }

        private static string ApplySignatureStatus(string html, bool mangakaSigned)
        {
            var platformSignatureNote = "(Đã ký điện tử)";
            var mangakaSignatureNote = mangakaSigned ? "(Đã ký điện tử)" : "(Chưa ký điện tử)";

            html = html
                .Replace("{{PlatformSignatureNote}}", platformSignatureNote)
                .Replace("{{MangakaSignatureNote}}", mangakaSignatureNote);

            html = Regex.Replace(
                html,
                "(<div\\s+class=\"signature-title\"\\s*>\\s*ĐẠI DIỆN BÊN B\\s*</div>\\s*<div\\s+class=\"signature-note\"\\s*>)(.*?)(</div>)",
                $"$1{platformSignatureNote}$3",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            html = Regex.Replace(
                html,
                "(<div\\s+class=\"signature-title\"\\s*>\\s*ĐẠI DIỆN BÊN A\\s*</div>\\s*<div\\s+class=\"signature-note\"\\s*>)(.*?)(</div>)",
                $"$1{mangakaSignatureNote}$3",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return html;
        }

        private static string FormatPublicationSchedule(string? publicationSchedule)
        {
            return string.IsNullOrWhiteSpace(publicationSchedule)
                ? "Chưa cấu hình"
                : publicationSchedule.Trim();
        }

        private static string EnsurePublicationSchedulePlaceholder(string html)
        {
            if (html.Contains("{{PublicationSchedule}}", StringComparison.OrdinalIgnoreCase) ||
                html.Contains("{{PublishSchedule}}", StringComparison.OrdinalIgnoreCase))
            {
                return html;
            }

            const string scheduleItem = "<li><strong>Lịch xuất bản:</strong> {{PublicationSchedule}}</li>";
            var inserted = Regex.Replace(
                html,
                "(<li>\\s*<strong>\\s*Thể loại:\\s*</strong>\\s*\\{\\{SeriesGenre\\}\\}\\s*</li>)",
                "$1" + scheduleItem,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!string.Equals(inserted, html, StringComparison.Ordinal))
            {
                return inserted;
            }

            if (html.Contains("</body>", StringComparison.OrdinalIgnoreCase))
            {
                return html.Replace(
                    "</body>",
                    "<p><strong>Lịch xuất bản:</strong> {{PublicationSchedule}}</p></body>",
                    StringComparison.OrdinalIgnoreCase);
            }

            return html + "<p><strong>Lịch xuất bản:</strong> {{PublicationSchedule}}</p>";
        }

        private static string ApplyContractPrintStyles(string html)
        {
            const string printCss = @"
<style>
  @page { size: A4; margin: 24mm 22mm; }
  html, body {
    width: 100%;
    margin: 0;
    padding: 0;
    color: #000;
    background: #fff;
    font-family: 'Times New Roman', Times, serif;
    font-size: 12pt;
    line-height: 1.45;
    text-align: justify;
    overflow-wrap: break-word;
    word-break: normal;
  }
  p { margin: 0 0 7pt 0; }
  .header-group { text-align: center; margin: 0 0 14pt 0; page-break-inside: avoid; }
  .country-title { font-size: 12pt; font-weight: bold; margin: 0 0 3pt 0; }
  .motto-title { font-size: 12pt; font-weight: bold; text-decoration: underline; margin: 0; }
  .separator { width: 95pt; border-top: 1px solid #000; margin: 5pt auto 12pt auto; }
  .contract-title { font-size: 15pt; line-height: 1.25; font-weight: bold; text-align: center; text-transform: uppercase; margin: 10pt 0 12pt 0; }
  .date-location { font-size: 11pt; font-style: italic; text-align: right; margin: 0 0 14pt 0; }
  .section-header { font-size: 12pt; line-height: 1.3; font-weight: bold; text-transform: uppercase; margin: 12pt 0 6pt 0; page-break-after: avoid; }
  .article-header { font-size: 12pt; line-height: 1.3; font-weight: bold; text-decoration: underline; margin: 10pt 0 5pt 0; page-break-after: avoid; }
  .info-table { width: 100%; border-collapse: collapse; table-layout: auto; margin: 0 0 9pt 0; }
  .info-table td { padding: 2pt 0; vertical-align: top; text-align: left; font-size: 12pt; line-height: 1.35; }
  .info-table td.label { width: 34mm; min-width: 34mm; white-space: nowrap; font-weight: bold; padding-right: 8pt; }
  .content-list { margin: 3pt 0 8pt 0; padding-left: 17pt; }
  .content-list li { margin: 0 0 4pt 0; padding-left: 2pt; }
  .signature-table { width: 100%; table-layout: fixed; border-collapse: collapse; margin-top: 26pt; page-break-inside: avoid; }
  .signature-table td { width: 50%; text-align: center; vertical-align: top; padding: 0 10pt; }
  .signature-title { font-weight: bold; font-size: 12pt; margin-bottom: 3pt; }
  .signature-note { font-style: italic; font-size: 10.5pt; margin-bottom: 44pt; }
  .signature-name { font-weight: bold; font-size: 12pt; }
</style>";

            if (html.Contains("</head>", StringComparison.OrdinalIgnoreCase))
            {
                return html.Replace("</head>", printCss + "</head>", StringComparison.OrdinalIgnoreCase);
            }

            return printCss + html;
        }

        private static readonly object _wkHtmlToPdfLock = new object();
        private static IConverter? _wkHtmlToPdfConverter;

        private static IConverter GetOrCreateConverter()
        {
            if (_wkHtmlToPdfConverter == null)
            {
                lock (_wkHtmlToPdfLock)
                {
                    if (_wkHtmlToPdfConverter == null)
                    {
                        _wkHtmlToPdfConverter = new SynchronizedConverter(new PdfTools());
                    }
                }
            }
            return _wkHtmlToPdfConverter;
        }

        private static byte[] GeneratePdfBytes(string htmlContent)
        {
            var converter = GetOrCreateConverter();

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 24, Right = 28, Bottom = 24, Left = 28 },
                    DocumentTitle = "Hợp đồng xuất bản"
                },
                Objects = {
                    new ObjectSettings
                    {
                        PagesCount = true,
                        HtmlContent = htmlContent,
                        WebSettings = {
                            DefaultEncoding = "utf-8"
                        }
                    }
                }
            };

            return converter.Convert(doc);
        }
    }
}
