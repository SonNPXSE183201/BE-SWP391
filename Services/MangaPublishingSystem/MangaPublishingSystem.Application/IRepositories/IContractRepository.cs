using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        Task<Contract?> GetContractWithDetailsAsync(int id);
        Task<List<Contract>> GetContractsWithDetailsAsync();
    }
}