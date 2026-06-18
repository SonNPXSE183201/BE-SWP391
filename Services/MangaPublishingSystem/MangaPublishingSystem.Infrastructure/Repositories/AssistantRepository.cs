using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Assistant;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Infrastructure.Data;
using MangaPublishingSystem.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class AssistantRepository : IAssistantRepository
    {
        private readonly MangaPublishingDbContext _context;

        public AssistantRepository(MangaPublishingDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AssistantResponseDto>> GetActiveAssistantsAsync(AssistantFilterDto filter)
        {
            var query = _context.Users
                .Include(u => u.AssistantProfile)
                .Where(u => u.RoleId == 5 && u.Status == UserStatus.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                // Sử dụng extension hỗ trợ tìm kiếm không dấu
                query = query.WhereContainsUnsigned(u => u.FullName, filter.SearchTerm);
            }

            var projectedQuery = query.Select(u => new AssistantResponseDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                Status = u.Status.ToString(),
                PenName = u.PenName,
                PortfolioUrl = u.PortfolioUrl,
                Skills = u.Skills,
                
                // Properties from AssistantProfile if it exists
                SpecialtyTags = u.AssistantProfile != null ? u.AssistantProfile.SpecialtyTags : null,
                TotalCompletedTasks = u.AssistantProfile != null ? u.AssistantProfile.TotalCompletedTasks : 0,
                OnTimeRate = u.AssistantProfile != null ? u.AssistantProfile.OnTimeRate : 0,
                DisputeRate = u.AssistantProfile != null ? u.AssistantProfile.DisputeRate : 0,
                CurrentActiveTasks = u.AssistantProfile != null ? u.AssistantProfile.CurrentActiveTasks : 0,
                AverageRating = u.AssistantProfile != null ? u.AssistantProfile.AverageRating : 0
            });

            return await projectedQuery.ToPagedListAsync(filter.PageNumber, filter.PageSize);
        }
    }
}
