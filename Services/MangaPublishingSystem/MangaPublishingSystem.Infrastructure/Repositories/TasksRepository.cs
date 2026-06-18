using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;
using MangaPublishingSystem.Infrastructure.Extensions;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class TasksRepository : GenericRepository<Tasks>, ITasksRepository
    {
        public TasksRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Tasks>> GetAvailableTasksAsync(string? skill, int pageNumber, int pageSize)
        {
            var query = _dbSet.AsQueryable()
                .Include(t => t.Mangaka)
                .Include(t => t.Assistant)
                .Include(t => t.Region)
                    .ThenInclude(r => r.Page)
                .Where(t => t.Status == "Pending")
                .AsQueryable();

            // Lọc theo kỹ năng/từ khóa trong mô tả (Description) - không phân biệt hoa thường và dấu
            if (!string.IsNullOrWhiteSpace(skill))
            {
                query = query.WhereContainsUnsigned(t => t.Description!, skill);
            }

            // Sắp xếp theo thời gian tạo mới nhất
            query = query.OrderByDescending(t => t.CreateAt);

            return await query.ToPagedListAsync(pageNumber, pageSize);
        }
    }
}