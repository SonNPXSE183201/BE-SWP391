using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Infrastructure.Data;
using MangaPublishingSystem.Infrastructure.Extensions;
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
                .Where(x => x.RoleId == 5 && x.Status == UserStatus.Pending)
                .ToListAsync();
        }

        public async Task<PagedResult<User>> GetUsersFilteredPagedAsync(
            string? role,
            string? status,
            string? search,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Users.Include(u => u.Role).Include(u => u.AssignedEditor).AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                var roleName = role.Trim() switch
                {
                    "Assistant" => "Assistant",
                    "Mangaka" => "Mangaka",
                    "Editor" => "Tantou Editor",
                    "Board" => "Editorial Board",
                    _ => role.Trim()
                };

                query = query.Where(u => u.Role != null && u.Role.RoleName == roleName);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var userStatus))
            {
                query = query.Where(u => u.Status == userStatus);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    EF.Functions.Collate(u.FullName, "SQL_Latin1_General_CP1_CI_AI").Contains(term)
                    || EF.Functions.Collate(u.Email, "SQL_Latin1_General_CP1_CI_AI").Contains(term)
                    || EF.Functions.Collate(u.UserName, "SQL_Latin1_General_CP1_CI_AI").Contains(term)
                    || (u.PenName != null && EF.Functions.Collate(u.PenName, "SQL_Latin1_General_CP1_CI_AI").Contains(term)));
            }

            return await query
                .OrderByDescending(u => u.CreateAt)
                .ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> ExistsByUserNameAsync(string userName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == userName);
        }

        public async Task<bool> ExistsByPenNameAsync(string penName)
        {
            var normalized = penName.Trim().ToLower();
            return await _context.Users.AnyAsync(x =>
                x.PenName != null && x.PenName.ToLower() == normalized);
        }

        public async Task<User?> GetUserWithRoleByUsernameOrEmailAsync(string identifier)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.UserName == identifier ||
                    x.Email == identifier);
        }

        public async Task<User?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.AssignedEditor)
                .Include(u => u.AssistantProfile)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
