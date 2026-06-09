using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly MangaPublishingDbContext _context;

        public UserRepository(MangaPublishingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<User>> GetPendingAssistantsAsync()
        {
            return await _context.Users
                .Where(x => x.RoleId == 4 && x.Status == "Pending")
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}