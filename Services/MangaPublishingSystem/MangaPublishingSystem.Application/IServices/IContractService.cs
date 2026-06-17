using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Contracts;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IContractService : IGenericService<Contract>
    {
        Task<ContractDto> CreateContractAsync(CreateContractDto dto);
        Task<ContractDto> UpdateContractAsync(int id, UpdateContractDto dto);
        Task<ContractDto> GetContractByIdAsync(int id);
        Task<IEnumerable<ContractDto>> GetContractsAsync();
        System.Threading.Tasks.Task DeleteContractAsync(int id);
    }
}