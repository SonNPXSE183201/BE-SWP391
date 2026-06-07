using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<User?> GetUserWithRoleByUsernameOrEmailAsync(string identifier)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == identifier || u.Email == identifier);
        }
    }
}