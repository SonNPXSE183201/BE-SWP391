using System;
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

        public async Task<PagedResult<User>> GetUsersPagedAsync(string? role, string? status, string? search, int pageNumber, int pageSize)
        {
            var query = _context.Users.Include(x => x.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                if (int.TryParse(role, out var roleId))
                {
                    query = query.Where(x => x.RoleId == roleId);
                }
                else
                {
                    var normalizedRoleName = role.Trim().ToLower();
                    query = query.Where(x => x.Role.RoleName.ToLower() == normalizedRoleName);
                }
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<UserStatus>(status, true, out var userStatus))
                {
                    query = query.Where(x => x.Status == userStatus);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(x =>
                    x.UserName.ToLower().Contains(searchLower) ||
                    x.FullName.ToLower().Contains(searchLower) ||
                    x.Email.ToLower().Contains(searchLower) ||
                    (x.PenName != null && x.PenName.ToLower().Contains(searchLower)));
            }

            return await query.ToPagedListAsync(pageNumber, pageSize);
        }
    }
}