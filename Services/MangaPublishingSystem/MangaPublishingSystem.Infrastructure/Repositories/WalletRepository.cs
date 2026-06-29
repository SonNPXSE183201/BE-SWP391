using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<Wallet?> GetWalletByUserIdAsync(int userId)
        {
            return await _context.Wallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<Wallet?> GetPlatformTreasuryWalletAsync()
        {
            return await _context.Wallets
                .FirstOrDefaultAsync(w => w.Kind == Domain.Constants.WalletKinds.PlatformTreasury);
        }
    }
}