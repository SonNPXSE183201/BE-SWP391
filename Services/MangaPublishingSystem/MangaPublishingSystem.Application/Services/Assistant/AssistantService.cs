using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Assistant;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace MangaPublishingSystem.Application.Services.Assistant
{
    public class AssistantService : IAssistantService
    {
        private readonly IAssistantRepository _assistantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAssistantProfileRepository _assistantProfileRepository;
        private readonly IPortfolioSampleRepository _portfolioSampleRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ISeriesAssistantRepository _seriesAssistantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public AssistantService(
            IAssistantRepository assistantRepository,
            IUserRepository userRepository,
            IAssistantProfileRepository assistantProfileRepository,
            IPortfolioSampleRepository portfolioSampleRepository,
            ITransactionRepository transactionRepository,
            ISeriesAssistantRepository seriesAssistantRepository,
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _assistantRepository = assistantRepository;
            _userRepository = userRepository;
            _assistantProfileRepository = assistantProfileRepository;
            _portfolioSampleRepository = portfolioSampleRepository;
            _transactionRepository = transactionRepository;
            _seriesAssistantRepository = seriesAssistantRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<IEnumerable<AssistantInviteDto>> GetMyInvitesAsync(int assistantId)
        {
            var invites = await _seriesAssistantRepository.GetPendingInvitesByAssistantAsync(assistantId);
            return invites.Select(sa => new AssistantInviteDto
            {
                SeriesId = sa.SeriesId,
                SeriesTitle = sa.Series?.Title ?? "Không rõ dự án",
                CoverUrl = sa.Series?.CoverArtworkUrl,
                RoleInTeam = sa.RoleInTeam,
                Status = sa.Status,
                CreateAt = sa.CreateAt,

                // Series detail
                Genre = sa.Series?.Genre,
                Synopsis = sa.Series?.Synopsis,
                PublicationSchedule = sa.Series?.PublicationSchedule,
                SeriesStatus = sa.Series?.Status,

                // Mangaka info
                MangakaName = sa.Series?.Mangaka?.FullName,
                MangakaPenName = sa.Series?.Mangaka?.PenName,

                // Team info
                TeamSize = sa.Series?.SeriesAssistants?.Count(m => m.Status == "Active") ?? 0
            });
        }

        public async Task<PagedResult<AssistantResponseDto>> GetActiveAssistantsAsync(AssistantFilterDto filter)
        {
            string cacheKey = $"ActiveAssistants_{filter.PageNumber}_{filter.PageSize}_{filter.SearchTerm?.ToLower()}";

            if (!_cache.TryGetValue(cacheKey, out PagedResult<AssistantResponseDto>? pagedResult))
            {
                pagedResult = await _assistantRepository.GetActiveAssistantsAsync(filter);
                
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

                _cache.Set(cacheKey, pagedResult, cacheEntryOptions);
            }

            return pagedResult!;
        }

        public async Task<AssistantProfileDto> GetProfileAsync(int assistantId)
        {
            var user = await _userRepository.GetByIdAsync(assistantId);
            if (user == null || user.RoleId != 5)
            {
                throw new NotFoundException("Tài khoản trợ lý không tồn tại.");
            }

            var profiles = await _assistantProfileRepository.FindAsync(p => p.AssistantId == assistantId);
            var profile = profiles.FirstOrDefault();

            return new AssistantProfileDto
            {
                AssistantId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                PenName = user.PenName,
                PortfolioUrl = user.PortfolioUrl,
                Skills = user.Skills,
                SpecialtyTags = profile?.SpecialtyTags,
                TotalCompletedTasks = profile?.TotalCompletedTasks ?? 0,
                OnTimeRate = profile?.OnTimeRate ?? 0,
                DisputeRate = profile?.DisputeRate ?? 0,
                CurrentActiveTasks = profile?.CurrentActiveTasks ?? 0,
                AverageRating = profile?.AverageRating ?? 0
            };
        }

        public async Task<AssistantProfileDto> UpdateProfileAsync(int assistantId, UpdateAssistantProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(assistantId);
            if (user == null || user.RoleId != 5)
            {
                throw new NotFoundException("Tài khoản trợ lý không tồn tại.");
            }

            user.PortfolioUrl = dto.PortfolioUrl;
            user.Skills = dto.Skills;
            _userRepository.Update(user);

            var profiles = await _assistantProfileRepository.FindAsync(p => p.AssistantId == assistantId);
            var profile = profiles.FirstOrDefault();

            if (profile == null)
            {
                profile = new AssistantProfile
                {
                    AssistantId = assistantId,
                    SpecialtyTags = dto.SpecialtyTags,
                    TotalCompletedTasks = 0,
                    OnTimeRate = 0,
                    DisputeRate = 0,
                    CurrentActiveTasks = 0,
                    AverageRating = 0
                };
                await _assistantProfileRepository.AddAsync(profile);
            }
            else
            {
                profile.SpecialtyTags = dto.SpecialtyTags;
                _assistantProfileRepository.Update(profile);
            }

            await _unitOfWork.SaveChangesAsync();

            return new AssistantProfileDto
            {
                AssistantId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                PenName = user.PenName,
                PortfolioUrl = user.PortfolioUrl,
                Skills = user.Skills,
                SpecialtyTags = profile.SpecialtyTags,
                TotalCompletedTasks = profile.TotalCompletedTasks,
                OnTimeRate = profile.OnTimeRate,
                DisputeRate = profile.DisputeRate,
                CurrentActiveTasks = profile.CurrentActiveTasks,
                AverageRating = profile.AverageRating
            };
        }

        public async Task<AssistantPortfolioStatsDto> GetPortfolioStatsAsync(int assistantId)
        {
            var user = await _userRepository.GetByIdAsync(assistantId);
            if (user == null || user.RoleId != 5)
            {
                throw new NotFoundException("Tài khoản trợ lý không tồn tại.");
            }

            var profiles = await _assistantProfileRepository.FindAsync(p => p.AssistantId == assistantId);
            var profile = profiles.FirstOrDefault();
            if (profile == null)
            {
                throw new NotFoundException("Hồ sơ chi tiết trợ lý không tồn tại.");
            }

            var txs = await _transactionRepository.FindAsync(t => t.ToUserId == assistantId && t.Type == "Task_Payment" && t.Status == "Success");
            decimal totalEarnings = txs.Sum(t => t.Amount);

            return new AssistantPortfolioStatsDto
            {
                TotalCompletedTasks = profile.TotalCompletedTasks,
                OnTimeRate = profile.OnTimeRate,
                DisputeRate = profile.DisputeRate,
                CurrentActiveTasks = profile.CurrentActiveTasks,
                AverageRating = profile.AverageRating,
                TotalEarnings = totalEarnings
            };
        }

        public async Task<IEnumerable<PortfolioSampleDto>> GetPortfolioSamplesAsync(int assistantId)
        {
            var user = await _userRepository.GetByIdAsync(assistantId);
            if (user == null || user.RoleId != 5)
            {
                throw new NotFoundException("Tài khoản trợ lý không tồn tại.");
            }

            var samples = await _portfolioSampleRepository.FindAsync(s => s.AssistantId == assistantId);
            return samples.Select(s => new PortfolioSampleDto
            {
                Id = s.Id,
                AssistantId = s.AssistantId,
                Title = s.Title,
                ImageUrl = s.ImageUrl,
                Category = s.Category,
                CreateAt = s.CreateAt,
                UpdateAt = s.UpdateAt
            }).ToList();
        }

        public async Task<PortfolioSampleDto> CreatePortfolioSampleAsync(int assistantId, CreatePortfolioSampleDto dto)
        {
            var user = await _userRepository.GetByIdAsync(assistantId);
            if (user == null || user.RoleId != 5)
            {
                throw new NotFoundException("Tài khoản trợ lý không tồn tại.");
            }

            var sample = new PortfolioSample
            {
                AssistantId = assistantId,
                Title = dto.Title,
                ImageUrl = dto.ImageUrl,
                Category = dto.Category
            };

            await _portfolioSampleRepository.AddAsync(sample);
            await _unitOfWork.SaveChangesAsync();

            return new PortfolioSampleDto
            {
                Id = sample.Id,
                AssistantId = sample.AssistantId,
                Title = sample.Title,
                ImageUrl = sample.ImageUrl,
                Category = sample.Category,
                CreateAt = sample.CreateAt,
                UpdateAt = sample.UpdateAt
            };
        }

        public async Task<PortfolioSampleDto> UpdatePortfolioSampleAsync(int assistantId, int sampleId, UpdatePortfolioSampleDto dto)
        {
            var sample = await _portfolioSampleRepository.GetByIdAsync(sampleId);
            if (sample == null || sample.AssistantId != assistantId)
            {
                throw new NotFoundException("Mẫu vẽ không tồn tại hoặc không thuộc quyền sở hữu của bạn.");
            }

            sample.Title = dto.Title;
            sample.ImageUrl = dto.ImageUrl;
            sample.Category = dto.Category;

            _portfolioSampleRepository.Update(sample);
            await _unitOfWork.SaveChangesAsync();

            return new PortfolioSampleDto
            {
                Id = sample.Id,
                AssistantId = sample.AssistantId,
                Title = sample.Title,
                ImageUrl = sample.ImageUrl,
                Category = sample.Category,
                CreateAt = sample.CreateAt,
                UpdateAt = sample.UpdateAt
            };
        }

        public async Task DeletePortfolioSampleAsync(int assistantId, int sampleId)
        {
            var sample = await _portfolioSampleRepository.GetByIdAsync(sampleId);
            if (sample == null || sample.AssistantId != assistantId)
            {
                throw new NotFoundException("Mẫu vẽ không tồn tại hoặc không thuộc quyền sở hữu của bạn.");
            }

            _portfolioSampleRepository.Delete(sample);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
