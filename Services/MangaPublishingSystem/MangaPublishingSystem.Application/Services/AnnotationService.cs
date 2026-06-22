using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Annotations;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class AnnotationService : GenericService<Annotation>, IAnnotationService
    {
        private readonly IAnnotationRepository _annotationRepository;
        private readonly IPageRepository _pageRepository;
        private readonly ITaskVersionRepository _taskVersionRepository;

        public AnnotationService(
            IAnnotationRepository repository, 
            IPageRepository pageRepository,
            ITaskVersionRepository taskVersionRepository,
            IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
            _annotationRepository = repository;
            _pageRepository = pageRepository;
            _taskVersionRepository = taskVersionRepository;
        }

        public async Task<IEnumerable<AnnotationDto>> GetAnnotationsAsync(int? pageId, int? taskVersionId)
        {
            var annotations = await _annotationRepository.GetAnnotationsWithDetailsAsync(pageId, taskVersionId);
            var dtos = new List<AnnotationDto>();
            foreach (var a in annotations)
            {
                dtos.Add(MapToDto(a));
            }
            return dtos;
        }

        public async Task<AnnotationDto> GetByIdDtoAsync(int id)
        {
            var annotation = await _annotationRepository.GetAnnotationWithDetailsByIdAsync(id);
            if (annotation == null)
            {
                throw new NotFoundException("Chú thích không tồn tại.");
            }
            return MapToDto(annotation);
        }

        public async Task<AnnotationDto> CreateAnnotationAsync(int userId, CreateAnnotationDto dto)
        {
            if (dto.PageId.HasValue)
            {
                var page = await _pageRepository.GetByIdAsync(dto.PageId.Value);
                if (page == null)
                {
                    throw new NotFoundException("Trang truyện không tồn tại.");
                }
            }

            if (dto.TaskVersionId.HasValue)
            {
                var taskVersion = await _taskVersionRepository.GetByIdAsync(dto.TaskVersionId.Value);
                if (taskVersion == null)
                {
                    throw new NotFoundException("Phiên bản nhiệm vụ không tồn tại.");
                }
            }

            var annotation = new Annotation
            {
                CreatedByUserId = userId,
                PageId = dto.PageId,
                TaskVersionId = dto.TaskVersionId,
                CoordinatesJson = dto.CoordinatesJson,
                Comment = dto.Comment,
                Type = dto.Type
            };

            await _repository.AddAsync(annotation);
            await _unitOfWork.SaveChangesAsync();

            var details = await _annotationRepository.GetAnnotationWithDetailsByIdAsync(annotation.Id);
            if (details == null)
            {
                throw new NotFoundException("Không thể tải chi tiết chú thích sau khi tạo.");
            }

            return MapToDto(details);
        }

        public async Task<AnnotationDto> UpdateAnnotationAsync(int userId, int id, UpdateAnnotationDto dto)
        {
            var annotation = await _annotationRepository.GetAnnotationWithDetailsByIdAsync(id);
            if (annotation == null)
            {
                throw new NotFoundException("Chú thích không tồn tại.");
            }

            if (annotation.CreatedByUserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền chỉnh sửa chú thích của người khác.");
            }

            annotation.CoordinatesJson = dto.CoordinatesJson;
            annotation.Comment = dto.Comment;
            annotation.Type = dto.Type;

            _repository.Update(annotation);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(annotation);
        }

        public async Task DeleteAnnotationAsync(int userId, int id)
        {
            var annotation = await _repository.GetByIdAsync(id);
            if (annotation == null)
            {
                throw new NotFoundException("Chú thích không tồn tại.");
            }

            if (annotation.CreatedByUserId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền xóa chú thích của người khác.");
            }

            _repository.Delete(annotation);
            await _unitOfWork.SaveChangesAsync();
        }

        private static AnnotationDto MapToDto(Annotation annotation)
        {
            return new AnnotationDto
            {
                Id = annotation.Id,
                CreatedByUserId = annotation.CreatedByUserId,
                CreatedByUserName = annotation.CreatedByUser?.FullName ?? "Unknown",
                CreatedByUserRole = annotation.CreatedByUser?.Role?.RoleName ?? "Unknown",
                PageId = annotation.PageId,
                TaskVersionId = annotation.TaskVersionId,
                CoordinatesJson = annotation.CoordinatesJson,
                Comment = annotation.Comment,
                Type = annotation.Type,
                CreateAt = annotation.CreateAt,
                UpdateAt = annotation.UpdateAt
            };
        }
    }
}