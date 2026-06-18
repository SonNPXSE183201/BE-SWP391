using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Application.DTOs.Pages;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class PageRepository : GenericRepository<Page>, IPageRepository
    {
        public PageRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<List<LayerDto>> GetPageLayersAsync(int pageId)
        {
            var regionsWithTasks = await _context.Regions
                .Where(r => r.PageId == pageId)
                .Select(r => new
                {
                    Region = r,
                    LatestTask = r.Tasks
                        .OrderByDescending(t => t.CreateAt)
                        .Select(t => new
                        {
                            t.Status,
                            t.ZIndex_Order,
                            LatestVersion = t.TaskVersions
                                .OrderByDescending(tv => tv.VersionNumber)
                                .Select(tv => tv.SubmittedFileUrl)
                                .FirstOrDefault()
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return regionsWithTasks.Select(x => new LayerDto
            {
                RegionId = x.Region.Id,
                RegionName = x.Region.Name,
                CoordinatesJson = x.Region.CoordinatesJson,
                ZIndex_Order = x.LatestTask?.ZIndex_Order ?? 0,
                ImageUrl = x.LatestTask?.LatestVersion,
                TaskStatus = x.LatestTask?.Status ?? "None"
            }).ToList();
        }
    }
}