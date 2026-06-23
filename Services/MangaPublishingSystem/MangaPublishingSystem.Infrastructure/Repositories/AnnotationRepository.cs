using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<Annotation>> GetAnnotationsWithDetailsAsync(int? pageId, int? taskVersionId)
        {
            var query = _context.Annotations
                .Include(a => a.CreatedByUser)
                    .ThenInclude(u => u.Role)
                .AsQueryable();

            if (pageId.HasValue)
            {
                query = query.Where(a => a.PageId == pageId.Value);
            }

            if (taskVersionId.HasValue)
            {
                query = query.Where(a => a.TaskVersionId == taskVersionId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Annotation?> GetAnnotationWithDetailsByIdAsync(int id)
        {
            return await _context.Annotations
                .Include(a => a.CreatedByUser)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}