using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<List<User>> GetPendingAssistantsAsync()
        {
            return await _context.Users
                .Where(x => x.RoleId == 4 && x.Status == UserStatus.Pending)
                .ToListAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> ExistsByUserNameAsync(string userName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == userName);
        }

        public async Task<User?> GetUserWithRoleByUsernameOrEmailAsync(string identifier)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.UserName == identifier ||
                    x.Email == identifier);
        }
    }
}