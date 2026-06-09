<<<<<<< HEAD
=======
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Domain.Entities;
>>>>>>> origin/dev
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
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == identifier || u.Email == identifier);
        }
    }
}