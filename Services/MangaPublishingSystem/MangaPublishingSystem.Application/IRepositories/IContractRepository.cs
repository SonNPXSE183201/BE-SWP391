using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        Task<List<Series>> GetApprovedSeriesForContractsAsync();

        Task<Contract?> GetBySeriesIdAsync(int seriesId);

        Task<Contract?> GetSignedWithAddendumsBySeriesIdAsync(int seriesId);

        /// <summary>Hợp đồng đang hiệu lực (Signed hoặc Active) kèm phụ lục.</summary>
        Task<Contract?> GetEffectiveContractWithAddendumsBySeriesIdAsync(int seriesId);

        Task<Contract?> GetWithSeriesAsync(int contractId);

        System.Threading.Tasks.Task AddAddendumAsync(ContractAddendum addendum);

        Task<Contract?> GetContractWithDetailsAsync(int id);

        Task<List<Contract>> GetContractsWithDetailsAsync();
    }
}
