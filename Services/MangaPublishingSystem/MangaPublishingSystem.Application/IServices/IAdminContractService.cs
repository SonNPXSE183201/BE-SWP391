using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Admin;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IAdminContractService
    {
        Task<List<ApprovedSeriesContractDto>> GetApprovedSeriesAsync();

        Task<CreateContractResponseDto> CreateContractAsync(CreateContractRequestDto dto);

        System.Threading.Tasks.Task UpdateContractAsync(int contractId, UpdateContractRequestDto dto);
    }
}
