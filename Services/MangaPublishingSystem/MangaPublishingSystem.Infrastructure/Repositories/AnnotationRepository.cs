using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class AnnotationRepository : GenericRepository<Annotation>, IAnnotationRepository
    {
        public AnnotationRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}