using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<List<User>> GetPendingAssistantsAsync();

        Task<List<User>> GetUsersFilteredAsync(string? role, string? status);

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByUserNameAsync(string userName);

        Task<bool> ExistsByPenNameAsync(string penName);

        Task<User?> GetUserWithRoleByUsernameOrEmailAsync(string identifier);

        Task<User?> GetByIdWithDetailsAsync(int id);
    }
}
