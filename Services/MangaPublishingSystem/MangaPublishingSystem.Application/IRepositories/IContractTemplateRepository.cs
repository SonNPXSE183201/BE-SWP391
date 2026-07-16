using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IContractTemplateRepository : IGenericRepository<ContractTemplate>
    {
        Task<ContractTemplate?> GetActiveTemplateAsync();
    }
}
