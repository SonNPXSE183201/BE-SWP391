using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
<<<<<<< HEAD
        Task<List<User>> GetPendingAssistantsAsync();

        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUserNameAsync(string userName);
=======
        Task<User?> GetUserWithRoleByUsernameOrEmailAsync(string identifier);
>>>>>>> origin/dev
    }
}