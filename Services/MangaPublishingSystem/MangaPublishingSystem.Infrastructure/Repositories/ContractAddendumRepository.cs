using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class ContractAddendumRepository : GenericRepository<ContractAddendum>, IContractAddendumRepository
    {
        public ContractAddendumRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}