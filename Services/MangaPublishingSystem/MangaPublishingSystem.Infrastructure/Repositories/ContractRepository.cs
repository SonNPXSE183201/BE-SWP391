using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<Contract?> GetContractWithDetailsAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.User)
                .Include(c => c.Series)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Contract>> GetContractsWithDetailsAsync()
        {
            return await _context.Contracts
                .Include(c => c.User)
                .Include(c => c.Series)
                .ToListAsync();
        }
    }
}