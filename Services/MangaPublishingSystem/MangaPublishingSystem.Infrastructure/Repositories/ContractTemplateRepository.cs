using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class ContractTemplateRepository : GenericRepository<ContractTemplate>, IContractTemplateRepository
    {
        public ContractTemplateRepository(MangaPublishingDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ContractTemplate?> GetActiveTemplateAsync()
        {
            return await _dbSet.Where(t => t.IsActive).OrderByDescending(t => t.Id).FirstOrDefaultAsync();
        }
    }
}
