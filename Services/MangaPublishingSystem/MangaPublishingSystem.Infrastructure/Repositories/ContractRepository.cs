using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<List<Series>> GetApprovedSeriesForContractsAsync()
        {
            return await _context.Series
                .Include(s => s.Mangaka)
                .Include(s => s.Contracts)
                .Where(s => s.Status == "Board_Approved" || s.Status == "Approved")
                .OrderByDescending(s => s.UpdateAt ?? s.CreateAt)
                .ToListAsync();
        }

        public async Task<Contract?> GetBySeriesIdAsync(int seriesId)
        {
            return await _context.Contracts.FirstOrDefaultAsync(c => c.SeriesId == seriesId);
        }

        public async Task<Contract?> GetWithSeriesAsync(int contractId)
        {
            return await _context.Contracts
                .Include(c => c.Series)
                .Include(c => c.User)
                .Include(c => c.ContractAddendums)
                .FirstOrDefaultAsync(c => c.Id == contractId);
        }

        public async System.Threading.Tasks.Task AddAddendumAsync(ContractAddendum addendum)
        {
            await _context.ContractAddendums.AddAsync(addendum);
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
