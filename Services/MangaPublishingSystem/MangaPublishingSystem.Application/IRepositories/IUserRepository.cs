using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
           Task<List<User>> GetPendingAssistantsAsync();       
           Task<User?> GetByIdAsync(int id);
    }
}