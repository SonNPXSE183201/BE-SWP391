using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using MangaPublishingSystem.Application.DTOs.Annotations;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IAnnotationService : IGenericService<Annotation>
    {
        Task<IEnumerable<AnnotationDto>> GetAnnotationsAsync(int? pageId, int? taskVersionId);
        Task<AnnotationDto> GetByIdDtoAsync(int id);
        Task<AnnotationDto> CreateAnnotationAsync(int userId, CreateAnnotationDto dto);
        Task<AnnotationDto> UpdateAnnotationAsync(int userId, int id, UpdateAnnotationDto dto);
        Task DeleteAnnotationAsync(int userId, int id);
    }
}