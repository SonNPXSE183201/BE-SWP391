using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<List<User>> GetPendingAssistantsAsync();

        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUserNameAsync(string userName);
    }
}