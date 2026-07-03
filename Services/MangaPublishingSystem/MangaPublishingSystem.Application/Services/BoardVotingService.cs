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
            var boardMembers = await GetActiveBoardMembersAsync(config);
            return await MapConfigDto(config, boardMembers);
        }

        public async Task<BoardVotingConfigDto> UpdateConfigAsync(UpdateBoardVotingConfigDto dto)
        {
            if (dto.ApprovalThresholdPercent is < 1 or > 100)
            {
                throw new BadRequestException("Ngưỡng phần trăm phải từ 1 đến 100.");
            }

            if (dto.AutoResolveHours < 1)
            {
                throw new BadRequestException("Thời hạn tự chốt phải ít nhất 1 giờ.");
            }

            var config = await GetConfigAsync();
            var boardMembers = await GetActiveBoardMembersAsync(config);

            if (dto.ChairUserId.HasValue &&
                boardMembers.All(m => m.Id != dto.ChairUserId.Value))
            {
                throw new BadRequestException(
                    "Chủ tịch Hội đồng phải là thành viên HĐ đang hoạt động (Active). Tài khoản bị khóa không thể làm Chủ tịch.");
            }

            config.AutoResolveHours = dto.AutoResolveHours;
            config.ApprovalThresholdPercent = dto.ApprovalThresholdPercent;
            config.ClearVotesOnResubmit = dto.ClearVotesOnResubmit;
            config.ChairUserId = dto.ChairUserId;
            _configRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();

            await _notificationPublisher.PublishBoardDataChangedAsync();
            return await MapConfigDto(config, boardMembers);
        }

        public async System.Threading.Tasks.Task ClearChairIfUserDeactivatedAsync(int userId)
        {
            var config = await GetConfigAsync();
            if (!config.ChairUserId.HasValue || config.ChairUserId.Value != userId)
            {
                return;
            }

            config.ChairUserId = null;
            _configRepository.Update(config);
            await _unitOfWork.SaveChangesAsync();
            await _notificationPublisher.PublishBoardDataChangedAsync();
        }

        public async System.Threading.Tasks.Task NotifyBoardMembershipChangedAsync(int userId)
        {
            var config = await GetConfigAsync();
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.RoleId != config.BoardRoleId)
            {
                return;
            }

            await _notificationPublisher.PublishBoardDataChangedAsync();
        }

        public async Task<BoardVotingRulesDto> BuildRulesDtoAsync()
        {
            var config = await GetConfigAsync();
            var boardMembers = await GetActiveBoardMembersAsync(config);
            var thresholds = BoardVotingRulesCalculator.CalculateThresholds(boardMembers.Count, config);
            return await MapRulesDto(config, thresholds, boardMembers);
        }

        public async Task<PendingBoardVotesResponseDto> GetPendingVotesPayloadAsync(int boardMemberId)
        {
            var rules = await BuildRulesDtoAsync();

            var pendingSeries = (await _seriesRepository.FindWithDetailsAsync(s => s.Status == "Pending_Board_Vote"))
                .ToList();

            var myApprovalVotes = (await _boardVoteRepository.FindAsync(v => v.BoardMemberId == boardMemberId))
                .Where(v => IsBoardApprovalVoteType(v.VoteType))
                .ToList();

            var votedSeriesIds = myApprovalVotes
                .Select(v => v.SeriesId)
                .Distinct()
                .ToHashSet();

            var resolvedVotedSeries = votedSeriesIds.Count == 0
                ? new List<Series>()
                : (await _seriesRepository.FindWithDetailsAsync(s =>
                    votedSeriesIds.Contains(s.Id) && s.Status != "Pending_Board_Vote")).ToList();

            var seriesList = pendingSeries
                .Concat(resolvedVotedSeries)
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .ToList();

            var seriesIds = seriesList.Select(s => s.Id).ToList();
            List<BoardVote> votes;
            if (seriesIds.Count == 0)
            {
                votes = new List<BoardVote>();
            }
            else
            {
                votes = (await _boardVoteRepository.FindAsync(v => seriesIds.Contains(v.SeriesId))).ToList();
            }

            var seriesDtos = seriesList
                .Select(s => MapSeriesDto(s, votes))
                .OrderByDescending(s => s.UpdateAt)
                .ToList();

            return new PendingBoardVotesResponseDto
            {
                Rules = rules,
                Series = seriesDtos
            };
        }

        private static bool IsBoardApprovalVoteType(string? voteType)
        {
            var type = voteType?.Trim() ?? string.Empty;
            return type.Equals("Approve", StringComparison.OrdinalIgnoreCase)
                || type.Equals("Reject", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IEnumerable<SeriesDto>> GetEscalatedSeriesAsync()
        {
            return Array.Empty<SeriesDto>();
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
                throw new ConflictException("Chỉ có thể quyết định thủ công khi biểu quyết đang treo.");
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                throw new BadRequestException("Vui lòng ghi lý do quyết định thủ công.");
            }

            if (dto.Approved)
            {
                var config = await GetConfigAsync();
                var boardMembers = await GetActiveBoardMembersAsync(config);
                var thresholds = BoardVotingRulesCalculator.CalculateThresholds(boardMembers.Count, config);
                var allVotes = (await _boardVoteRepository.FindAsync(v => v.SeriesId == seriesId)).ToList();
                var effectiveChairId = ResolveEffectiveChairUserId(config.ChairUserId, boardMembers);
                var budget = dto.ApprovedBudget
                    ?? BoardVotingRulesCalculator.CalculateApprovedBudget(
                        allVotes, effectiveChairId, thresholds.ChairWeight);

                if (budget <= 0)
                {
                    budget = series.EstimatedProductionBudget;
                }

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
            var effectiveChairId = ResolveEffectiveChairUserId(config.ChairUserId, boardMembers);
            var (approve, reject) = BoardVotingRulesCalculator.CountWeightedVotes(
                votes, effectiveChairId, thresholds.ChairWeight);

            return BoardVotingRulesCalculator.Evaluate(
                thresholds, approve, reject, votes.Count, boardMembers.Count);
        }

        public async System.Threading.Tasks.Task ApplyVoteResolutionAsync(Series series, BoardVoteResolution resolution, string? rejectComment = null)
        {
            switch (resolution)
            {
                case BoardVoteResolution.Approved:
                {
                    if (series.Status != "Fund_Pending")
                    {
                        var config = await GetConfigAsync();
                        var boardMembers = await GetActiveBoardMembersAsync(config);
                        var thresholds = BoardVotingRulesCalculator.CalculateThresholds(boardMembers.Count, config);
                        var votes = (await _boardVoteRepository.FindAsync(v => v.SeriesId == series.Id)).ToList();
                        var effectiveChairId = ResolveEffectiveChairUserId(config.ChairUserId, boardMembers);
                        var approvedBudget = BoardVotingRulesCalculator.CalculateApprovedBudget(
                            votes, effectiveChairId, thresholds.ChairWeight);

                        if (approvedBudget <= 0)
                        {
                            approvedBudget = series.EstimatedProductionBudget;
                        }

                        series.Status = "Fund_Pending";
                        series.ApprovedProductionBudget = approvedBudget;
                        _seriesRepository.Update(series);

                        var content =
                            $"Bộ truyện '{series.Title}' đã được phê duyệt cấp vốn với ngân sách {approvedBudget:N0} VND. Vui lòng xác nhận nhận gói vốn.";
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

        private async Task<BoardVotingConfigDto> MapConfigDto(BoardVotingConfig config, List<User> boardMembers)
        {
            string? chairName = null;
            if (config.ChairUserId.HasValue)
            {
                var chair = boardMembers.FirstOrDefault(u => u.Id == config.ChairUserId.Value)
                    ?? await _userRepository.GetByIdAsync(config.ChairUserId.Value);
                chairName = chair?.FullName;
            }

            var (chairIsValid, chairWarning, effectiveChairId) = ResolveChairState(config.ChairUserId, boardMembers, chairName);

            return new BoardVotingConfigDto
            {
                AutoResolveHours = config.AutoResolveHours,
                ApprovalThresholdPercent = config.ApprovalThresholdPercent,
                ClearVotesOnResubmit = config.ClearVotesOnResubmit,
                BoardRoleId = config.BoardRoleId,
                ChairUserId = config.ChairUserId,
                ChairUserName = chairName,
                ChairIsValid = chairIsValid,
                ChairInvalidWarning = chairWarning
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

            var (chairIsValid, chairWarning, effectiveChairId) = ResolveChairState(config.ChairUserId, boardMembers, chairName);

            var chairSummary = chairIsValid && !string.IsNullOrWhiteSpace(chairName)
                ? $"Chủ tịch HĐ = {thresholds.ChairWeight} phiếu ({chairName}). "
                : $"Chủ tịch HĐ = {thresholds.ChairWeight} phiếu. ";

            return new BoardVotingRulesDto
            {
                BoardMemberCount = thresholds.BoardMemberCount,
                ApproveRequired = thresholds.ApproveRequired,
                TotalWeight = thresholds.TotalWeight,
                ChairWeight = thresholds.ChairWeight,
                ApprovalThresholdPercent = config.ApprovalThresholdPercent,
                AutoResolveHours = config.AutoResolveHours,
                ChairUserId = config.ChairUserId,
                ChairUserName = chairName,
                ChairIsValid = chairIsValid,
                ChairInvalidWarning = chairWarning,
                EffectiveChairUserId = effectiveChairId,
                RulesSummary =
                    $"Cần ≥{thresholds.ApproveRequired}/{thresholds.TotalWeight} trọng số phiếu Đồng ý ({config.ApprovalThresholdPercent}%). " +
                    chairSummary +
                    "TV thường = 1 phiếu. " +
                    $"Tự chốt sau {config.AutoResolveHours}h nếu chưa đủ ngưỡng."
            };
        }

        private static int? ResolveEffectiveChairUserId(int? chairUserId, List<User> activeBoardMembers)
        {
            if (!chairUserId.HasValue)
            {
                return null;
            }

            return activeBoardMembers.Any(m => m.Id == chairUserId.Value) ? chairUserId : null;
        }

        private static (bool IsValid, string? Warning, int? EffectiveChairUserId) ResolveChairState(
            int? chairUserId,
            List<User> activeBoardMembers,
            string? chairName)
        {
            if (!chairUserId.HasValue)
            {
                return (true, null, null);
            }

            if (activeBoardMembers.Any(m => m.Id == chairUserId.Value))
            {
                return (true, null, chairUserId);
            }

            var label = string.IsNullOrWhiteSpace(chairName) ? $"User #{chairUserId}" : chairName;
            return (
                false,
                $"Chủ tịch HĐ ({label}) không còn hoạt động hoặc đã bị khóa. Vui lòng chỉ định Chủ tịch mới — hiện tại mọi TV được tính 1 phiếu.",
                null);
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
                EditorRecommendedBudget = series.EditorRecommendedBudget,
                ApprovedProductionBudget = series.ApprovedProductionBudget,
                PublicationSchedule = series.PublicationSchedule,
                Status = series.Status,
                ResourceFolderUrl = series.ResourceFolderUrl,
                MangakaName = series.Mangaka?.FullName,
                EditorName = series.Editor?.FullName,
                EditorNote = series.EditorNote,
                MangakaSubmissionNote = series.MangakaSubmissionNote,
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
