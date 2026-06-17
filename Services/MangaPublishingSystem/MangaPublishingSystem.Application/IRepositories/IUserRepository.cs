using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<List<User>> GetPendingAssistantsAsync();

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByUserNameAsync(string userName);

        Task<bool> ExistsByPenNameAsync(string penName);

        Task<User?> GetUserWithRoleByUsernameOrEmailAsync(string identifier);

        Task<PagedResult<User>> GetUsersPagedAsync(string? role, string? status, string? search, int pageNumber, int pageSize);
    }
}