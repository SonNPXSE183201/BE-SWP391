using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserWithRoleByUsernameOrEmailAsync(string identifier);
    }
}