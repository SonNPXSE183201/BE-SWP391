using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Contracts;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IContractTemplateService : IGenericService<ContractTemplate>
    {
        Task<IEnumerable<ContractTemplateDto>> GetTemplatesAsync();
        Task<ContractTemplateDto> GetTemplateByIdAsync(int id);
        Task<ContractTemplateDto> CreateTemplateAsync(CreateContractTemplateDto dto, int currentUserId);
        Task<ContractTemplateDto> UpdateTemplateAsync(int id, UpdateContractTemplateDto dto);
        System.Threading.Tasks.Task DeleteTemplateAsync(int id);
    }
}
