using System.Collections.Generic;
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
                        .ThenInclude(p => p.Chapter)
                            .ThenInclude(c => c.Series)
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

        public async Task<PagedResult<Tasks>> GetAssistantTasksAsync(int assistantId, string? status, int pageNumber, int pageSize)
        {
            var query = _dbSet.AsQueryable()
                .Include(t => t.Mangaka)
                .Include(t => t.Assistant)
                .Include(t => t.Region)
                    .ThenInclude(r => r.Page)
                        .ThenInclude(p => p.Chapter)
                            .ThenInclude(c => c.Series)
                .Where(t => t.AssistantId == assistantId);

            // Lọc theo trạng thái nhiệm vụ nếu được truyền vào
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            // Sắp xếp theo hạn chót gần nhất để ưu tiên việc cần làm trước
            query = query.OrderBy(t => t.Deadline);

            return await query.ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<Tasks>> GetMangakaTasksAsync(int mangakaId)
        {
            return await _dbSet.AsQueryable()
                .Include(t => t.Mangaka)
                .Include(t => t.Assistant)
                .Include(t => t.Region)
                    .ThenInclude(r => r.Page)
                        .ThenInclude(p => p.Chapter)
                            .ThenInclude(c => c.Series)
                .Where(t => t.MangakaId == mangakaId)
                .OrderByDescending(t => t.CreateAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Tasks?> GetTaskByIdWithDetailsAsync(int id)
        {
            return await _dbSet.AsQueryable()
                .Include(t => t.Mangaka)
                .Include(t => t.Assistant)
                .Include(t => t.Region)
                    .ThenInclude(r => r.Page)
                        .ThenInclude(p => p.Chapter)
                            .ThenInclude(c => c.Series)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}