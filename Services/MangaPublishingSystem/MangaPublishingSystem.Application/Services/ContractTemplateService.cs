using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Contracts;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using BuildingBlocks.Exceptions;

namespace MangaPublishingSystem.Application.Services
{
    public class ContractTemplateService : GenericService<ContractTemplate>, IContractTemplateService
    {
        private readonly IContractTemplateRepository _templateRepository;

        public ContractTemplateService(
            IContractTemplateRepository templateRepository,
            IUnitOfWork unitOfWork) : base(templateRepository, unitOfWork)
        {
            _templateRepository = templateRepository;
        }

        public async Task<IEnumerable<ContractTemplateDto>> GetTemplatesAsync()
        {
            var templates = await _templateRepository.GetAllAsync();
            return templates.Select(MapToDto);
        }

        public async Task<ContractTemplateDto> GetTemplateByIdAsync(int id)
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
                throw new NotFoundException("Không tìm thấy mẫu hợp đồng.");

            return MapToDto(template);
        }

        public async Task<ContractTemplateDto> CreateTemplateAsync(CreateContractTemplateDto dto, int currentUserId)
        {
            var templates = await _templateRepository.GetAllAsync();
            int newVersion = templates.Any() ? templates.Max(t => t.Version) + 1 : 1;

            if (dto.IsActive)
            {
                foreach (var t in templates.Where(t => t.IsActive))
                {
                    t.IsActive = false;
                    _templateRepository.Update(t);
                }
            }

            var newTemplate = new ContractTemplate
            {
                Content = dto.Content,
                IsActive = dto.IsActive,
                Version = newVersion,
                CreatedByUserId = currentUserId,
            };

            await _templateRepository.AddAsync(newTemplate);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(newTemplate);
        }

        public async Task<ContractTemplateDto> UpdateTemplateAsync(int id, UpdateContractTemplateDto dto)
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
                throw new NotFoundException("Không tìm thấy mẫu hợp đồng.");

            if (dto.IsActive && !template.IsActive)
            {
                var activeTemplates = await _templateRepository.FindAsync(t => t.IsActive && t.Id != id);
                foreach (var activeT in activeTemplates)
                {
                    activeT.IsActive = false;
                    _templateRepository.Update(activeT);
                }
            }

            template.Content = dto.Content;
            template.IsActive = dto.IsActive;

            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(template);
        }

        public async System.Threading.Tasks.Task DeleteTemplateAsync(int id)
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
                throw new NotFoundException("Không tìm thấy mẫu hợp đồng.");

            _templateRepository.Delete(template);
            await _unitOfWork.SaveChangesAsync();
        }

        private static ContractTemplateDto MapToDto(ContractTemplate entity)
        {
            return new ContractTemplateDto
            {
                Id = entity.Id,
                Content = EnsurePublicationSchedulePlaceholder(entity.Content),
                Version = entity.Version,
                IsActive = entity.IsActive,
                CreatedByUserId = entity.CreatedByUserId,
                CreateAt = entity.CreateAt,
                UpdateAt = entity.UpdateAt
            };
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
    }
}
