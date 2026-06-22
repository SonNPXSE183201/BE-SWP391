using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IAnnotationRepository : IGenericRepository<Annotation>
    {
        Task<IEnumerable<Annotation>> GetAnnotationsWithDetailsAsync(int? pageId, int? taskVersionId);
        Task<Annotation?> GetAnnotationWithDetailsByIdAsync(int id);
    }
}