using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class AnnotationService : GenericService<Annotation>, IAnnotationService
    {
        public AnnotationService(IAnnotationRepository repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
        }
    }
}