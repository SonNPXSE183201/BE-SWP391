using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.Common;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;

namespace MangaPublishingSystem.Application.Services
{
    public class BoardVotingService : IBoardVotingService
    {
        private readonly IBoardVotingConfigRepository _configRepository;
        private readonly IBoardVoteRepository _boardVoteRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IUnitOfWork _unitOfWork;

        public BoardVotingService(
            IBoardVotingConfigRepository configRepository,
            IBoardVoteRepository boardVoteRepository,
            ISeriesRepository seriesRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IUnitOfWork unitOfWork)
        {
            _configRepository = configRepository;
            _boardVoteRepository = boardVoteRepository;
            _seriesRepository = seriesRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _unitOfWork = unitOfWork;
        }

        public async Task<BoardVotingConfig> GetConfigAsync()
        {
            var configs = await _configRepository.GetAllAsync();
            var config = configs.FirstOrDefault();
            if (config != null)
            {
                return config;
            }

            config = new BoardVotingConfig { Id = 1 };
            await _configRepository.AddAsync(config);
            await _unitOfWork.SaveChangesAsync();
            return config;
        }

        public async Task<BoardVotingConfigDto> GetConfigDtoAsync()
        {
            var config = await GetConfigAsync();
            return await MapConfigDto(config);
        }

        public async Task<BoardVotingConfigDto> UpdateConfigAsync(UpdateBoardVotingConfigDto dto)
        {
            if (dto.ApprovalThresholdPercent is < 1 or > 100 ||
                dto.RejectionThresholdPercent is < 1 or > 100)
            {
                throw new BadRequestException("Ngưỡng phần trăm phải từ 1 đến 100.");
            }

            if (dto.AutoResolveHours < 1)
            {
                throw new BadRequestException("Thời hạn tự chốt phải ít nhất 1 giờ.");
            }

            var allowedPolicies = new[]
            {
                BoardVotingRulesCalculator.TiePolicyReject,
                BoardVotingRulesCalculator.TiePolicyEscalate,
                BoardVotingRulesCalculator.TiePolicyChairDecides
            };
            if (!allowedPolicies.Contains(dto.TiePolicy, StringComparer.OrdinalIgnoreCase))
            {
                throw new BadRequestException("TiePolicy không hợp lệ.");
            }

            var config = await GetConfigAsync();
            config.AutoResolveHours = dto.AutoResolveHours;
            config.ApprovalThresholdPercent = dto.ApprovalThresholdPercent;
            config.RejectionThresholdPercent = dto.RejectionThresholdPercent;
            config.TiePolicy = dto.TiePolicy;
            config.ClearVotesOnResubmit = dto.ClearVotesOnResubmit;
            config.RequireOddBoardSize = dto.RequireOddBoardSize;
            config.ChairUserId = dto.ChairUserId;
            _configRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            await _notificationPublisher.PublishBoardDataChangedAsync();
            return await MapConfigDto(config);
        }

        public async Task<BoardVotingRulesDto> BuildRulesDtoAsync()
        {
            var config = await GetConfigAsync();
            var boardMembers = await GetActiveBoardMembersAsync(config);
            var thresholds = BoardVotingRulesCalculator.CalculateThresholds(boardMembers.Count, config);
            return await MapRulesDto(config, thresholds, boardMembers);
        }

        public async Task<PendingBoardVotesResponseDto> GetPendingVotesPayloadAsync()
        {
            var rules = await BuildRulesDtoAsync();
            var seriesList = await _seriesRepository.FindAsync(s => s.Status == "Pending_Board_Vote");
            var seriesIds = seriesList.Select(s => s.Id).ToList();
            var votes = await _boardVoteRepository.FindAsync(v => seriesIds.Contains(v.SeriesId));

            var seriesDtos = seriesList.Select(s => MapSeriesDto(s, votes)).OrderByDescending(s => s.UpdateAt).ToList();
            return new PendingBoardVotesResponseDto
            {
                Rules = rules,
                Series = seriesDtos
            };
        }

        public async Task<IEnumerable<SeriesDto>> GetEscalatedSeriesAsync()
        {
            var seriesList = await _seriesRepository.FindAsync(s => s.Status == "Vote_Escalated");
            var seriesIds = seriesList.Select(s => s.Id).ToList();
            var votes = await _boardVoteRepository.FindAsync(v => seriesIds.Contains(v.SeriesId));
            return seriesList.Select(s => MapSeriesDto(s, votes)).OrderByDescending(s => s.UpdateAt).ToList();
        }

        public async System.Threading.Tasks.Task ManualResolveAsync(int seriesId, int adminUserId, ManualResolveBoardVoteDto dto)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "Vote_Escalated" && series.Status != "Pending_Board_Vote")
            {
                throw new ConflictException("Chỉ có thể quyết định thủ công khi biểu quyết đang treo hoặc đã leo thang.");
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                throw new BadRequestException("Vui lòng ghi lý do quyết định thủ công.");
            }

            if (dto.Approved)
            {
                var allVotes = (await _boardVoteRepository.FindAsync(v => v.SeriesId == seriesId)).ToList();
                var approveVotes = allVotes.Where(v => v.VoteType == "Approve").ToList();
                var budget = dto.ApprovedBudget
                    ?? (approveVotes.Any()
                        ? approveVotes.Average(v => v.RecommendedBudget)
                        : series.EstimatedProductionBudget);

                series.Status = "Fund_Pending";
                series.ApprovedProductionBudget = budget;
                _seriesRepository.Update(series);

                var content =
                    $"Bộ truyện '{series.Title}' đã được Quản trị viên phê duyệt thủ công. Ngân sách: {budget:N0} VND. Lý do: {dto.Reason}";
                await NotifyMangakaAsync(series, "Series_Approved_Manual", "Phê duyệt thủ công", content);
            }
            else
            {
                series.Status = "Rejected";
                _seriesRepository.Update(series);

                var content =
                    $"Bộ truyện '{series.Title}' bị từ chối bởi Quản trị viên (quyết định thủ công). Lý do: {dto.Reason}";
                await NotifyMangakaAsync(series, "Series_Rejected_Manual", "Từ chối thủ công", content);
            }

            await _unitOfWork.SaveChangesAsync();
            await _notificationPublisher.PublishBoardDataChangedAsync();
        }

        public async Task<BoardVoteResolution> EvaluateSeriesVotesAsync(int seriesId)
        {
            var config = await GetConfigAsync();
            var boardMembers = await GetActiveBoardMembersAsync(config);
            var thresholds = BoardVotingRulesCalculator.CalculateThresholds(boardMembers.Count, config);
            var votes = (await _boardVoteRepository.FindAsync(v => v.SeriesId == seriesId)).ToList();
            var (approve, reject, abstain) = BoardVotingRulesCalculator.CountVotes(votes);
            var votesCast = approve + reject + abstain;

            return BoardVotingRulesCalculator.Evaluate(
                thresholds, approve, reject, abstain, votesCast, config, votes);
        }

        public async System.Threading.Tasks.Task ApplyVoteResolutionAsync(Series series, BoardVoteResolution resolution, string? rejectComment = null)
        {
            switch (resolution)
            {
                case BoardVoteResolution.Approved:
                {
                    var votes = (await _boardVoteRepository.FindAsync(v => v.SeriesId == series.Id)).ToList();
                    var approveVotes = votes.Where(v => v.VoteType == "Approve").ToList();
                    var averageBudget = approveVotes.Any()
                        ? approveVotes.Average(v => v.RecommendedBudget)
                        : series.EstimatedProductionBudget;

                    if (series.Status != "Fund_Pending")
                    {
                        series.Status = "Fund_Pending";
                        series.ApprovedProductionBudget = averageBudget;
                        _seriesRepository.Update(series);

                        var content =
                            $"Bộ truyện '{series.Title}' đã được phê duyệt cấp vốn với ngân sách {averageBudget:N0} VND. Vui lòng xác nhận nhận gói vốn.";
                        await NotifyMangakaAsync(series, "Series_Approved", "Gói vốn đã được duyệt", content);
                    }
                    break;
                }
                case BoardVoteResolution.Rejected:
                {
                    if (series.Status != "Rejected")
                    {
                        series.Status = "Rejected";
                        _seriesRepository.Update(series);

                        var reason = string.IsNullOrWhiteSpace(rejectComment) ? "Không đạt ngưỡng biểu quyết." : rejectComment;
                        var content = $"Bộ truyện '{series.Title}' bị từ chối phê duyệt cấp vốn. {reason}";
                        await NotifyMangakaAsync(series, "Series_Rejected", "Bộ truyện bị từ chối", content);
                    }
                    break;
                }
                case BoardVoteResolution.Escalated:
                {
                    if (series.Status != "Vote_Escalated")
                    {
                        series.Status = "Vote_Escalated";
                        _seriesRepository.Update(series);

                        var content =
                            $"Bộ truyện '{series.Title}' đang hòa phiếu / chưa đạt ngưỡng. Quản trị viên sẽ quyết định thủ công.";
                        await NotifyMangakaAsync(series, "Series_Vote_Escalated", "Biểu quyết cần xử lý thủ công", content);
                    }
                    break;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await _notificationPublisher.PublishBoardDataChangedAsync();
        }

        public async System.Threading.Tasks.Task ClearVotesForSeriesAsync(int seriesId)
        {
            var votes = await _boardVoteRepository.FindAsync(v => v.SeriesId == seriesId);
            foreach (var vote in votes)
            {
                _boardVoteRepository.Delete(vote);
            }

            if (votes.Any())
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private async Task<List<User>> GetActiveBoardMembersAsync(BoardVotingConfig config)
        {
            var members = await _userRepository.FindAsync(u =>
                u.RoleId == config.BoardRoleId && u.Status == UserStatus.Active);
            return members.ToList();
        }

        private async Task<BoardVotingConfigDto> MapConfigDto(BoardVotingConfig config)
        {
            string? chairName = null;
            if (config.ChairUserId.HasValue)
            {
                var chair = await _userRepository.GetByIdAsync(config.ChairUserId.Value);
                chairName = chair?.FullName;
            }

            return new BoardVotingConfigDto
            {
                AutoResolveHours = config.AutoResolveHours,
                ApprovalThresholdPercent = config.ApprovalThresholdPercent,
                RejectionThresholdPercent = config.RejectionThresholdPercent,
                TiePolicy = config.TiePolicy,
                ClearVotesOnResubmit = config.ClearVotesOnResubmit,
                RequireOddBoardSize = config.RequireOddBoardSize,
                BoardRoleId = config.BoardRoleId,
                ChairUserId = config.ChairUserId,
                ChairUserName = chairName
            };
        }

        private async Task<BoardVotingRulesDto> MapRulesDto(
            BoardVotingConfig config,
            BoardVotingThresholds thresholds,
            List<User> boardMembers)
        {
            string? chairName = null;
            if (config.ChairUserId.HasValue)
            {
                var chair = boardMembers.FirstOrDefault(u => u.Id == config.ChairUserId.Value)
                    ?? await _userRepository.GetByIdAsync(config.ChairUserId.Value);
                chairName = chair?.FullName;
            }

            var tieLabel = config.TiePolicy switch
            {
                var p when p.Equals(BoardVotingRulesCalculator.TiePolicyReject, StringComparison.OrdinalIgnoreCase)
                    => "Hòa → tự động từ chối",
                var p when p.Equals(BoardVotingRulesCalculator.TiePolicyChairDecides, StringComparison.OrdinalIgnoreCase)
                    => "Hòa → Chủ tịch HĐ quyết định",
                _ => "Hòa → chuyển Admin xử lý thủ công"
            };

            return new BoardVotingRulesDto
            {
                BoardMemberCount = thresholds.BoardMemberCount,
                ApproveRequired = thresholds.ApproveRequired,
                RejectRequired = thresholds.RejectRequired,
                ApprovalThresholdPercent = config.ApprovalThresholdPercent,
                RejectionThresholdPercent = config.RejectionThresholdPercent,
                TiePolicy = config.TiePolicy,
                AutoResolveHours = config.AutoResolveHours,
                IsEvenBoardSize = thresholds.IsEvenBoardSize,
                RequireOddBoardSize = config.RequireOddBoardSize,
                OddBoardSizeWarning = thresholds.OddBoardSizeWarning,
                ChairUserId = config.ChairUserId,
                ChairUserName = chairName,
                RulesSummary =
                    $"Cần ≥{thresholds.ApproveRequired}/{thresholds.BoardMemberCount} phiếu Đồng ý ({config.ApprovalThresholdPercent}%) " +
                    $"hoặc ≥{thresholds.RejectRequired}/{thresholds.BoardMemberCount} phiếu Từ chối ({config.RejectionThresholdPercent}%). " +
                    $"{tieLabel}. Tự chốt sau {config.AutoResolveHours}h nếu chưa đủ ngưỡng."
            };
        }

        private static SeriesDto MapSeriesDto(Series series, IEnumerable<BoardVote> allVotes)
        {
            return new SeriesDto
            {
                Id = series.Id,
                MangakaId = series.MangakaId,
                EditorId = series.EditorId,
                Title = series.Title,
                Genre = series.Genre,
                Synopsis = series.Synopsis,
                CoverArtworkUrl = series.CoverArtworkUrl,
                EstimatedProductionBudget = series.EstimatedProductionBudget,
                ApprovedProductionBudget = series.ApprovedProductionBudget,
                PublicationSchedule = series.PublicationSchedule,
                Status = series.Status,
                ResourceFolderUrl = series.ResourceFolderUrl,
                MangakaName = series.Mangaka?.FullName,
                EditorName = series.Editor?.FullName,
                EditorNote = series.EditorNote,
                CreateAt = series.CreateAt,
                UpdateAt = series.UpdateAt,
                BoardVotes = allVotes.Where(v => v.SeriesId == series.Id).Select(v => new BoardVoteDto
                {
                    Id = v.Id,
                    SeriesId = v.SeriesId,
                    BoardMemberId = v.BoardMemberId,
                    VoteType = v.VoteType,
                    RecommendedBudget = v.RecommendedBudget,
                    Comment = v.Comment,
                    VoteAt = v.VoteAt
                }).ToList()
            };
        }

        private async System.Threading.Tasks.Task NotifyMangakaAsync(Series series, string type, string title, string content)
        {
            var notif = new Notification
            {
                UserId = series.MangakaId,
                Content = content,
                Type = type,
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync();

            await _notificationPublisher.PublishNotificationPayloadAsync(series.MangakaId, new NotificationPayload
            {
                Id = notif.Id,
                Title = title,
                Message = content,
                Link = $"/mangaka/series/{series.Id}",
                Type = type,
                CreateAt = notif.CreateAt
            });
        }
    }
}
