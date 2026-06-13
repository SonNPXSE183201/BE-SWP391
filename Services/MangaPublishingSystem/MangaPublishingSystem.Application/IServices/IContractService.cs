using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Contract;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IContractService : IGenericService<Contract>
    {
        System.Threading.Tasks.Task<ContractDto> CreateContractAsync(CreateContractDto dto);
        System.Threading.Tasks.Task AcceptContractAsync(int contractId, int mangakaId);
        System.Threading.Tasks.Task DeclineContractAsync(int contractId, int mangakaId, string declineReason);
    }
}