using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Dashboard;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Enums;

namespace MangaPublishingSystem.Application.Services.Dashboard
{
    public class DashboardStatsService : IDashboardStatsService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IWalletRepository _walletRepository;

        public DashboardStatsService(
            IUserRepository userRepository,
            ISeriesRepository seriesRepository,
            ITransactionRepository transactionRepository,
            ITasksRepository tasksRepository,
            IWalletRepository walletRepository)
        {
            _userRepository = userRepository;
            _seriesRepository = seriesRepository;
            _transactionRepository = transactionRepository;
            _tasksRepository = tasksRepository;
            _walletRepository = walletRepository;
        }

        public async Task<DashboardStatsResponseDto> GetStatsAsync(int userId, string roleName)
        {
            return roleName switch
            {
                "System Admin" => await BuildAdminStatsAsync(roleName),
                "Editorial Board" => await BuildBoardStatsAsync(roleName),
                "Tantou Editor" => await BuildEditorStatsAsync(userId, roleName),
                "Mangaka" => await BuildMangakaStatsAsync(userId, roleName),
                _ => throw new ForbiddenException("Vai trò của bạn không được phép truy cập thống kê dashboard.")
            };
        }

        private async Task<DashboardStatsResponseDto> BuildAdminStatsAsync(string roleName)
        {
            var users = (await _userRepository.GetAllAsync()).ToList();
            var series = (await _seriesRepository.GetAllAsync()).ToList();
            var transactions = (await _transactionRepository.GetAllAsync()).ToList();

            return new DashboardStatsResponseDto
            {
                Role = MapRoleForFe(roleName),
                Users = users.Count,
                PendingApprovals = users.Count(u => u.RoleId == 5 && u.Status == UserStatus.Pending),
                Series = series.Count,
                Transactions = transactions.Count
            };
        }

        private async Task<DashboardStatsResponseDto> BuildBoardStatsAsync(string roleName)
        {
            var series = (await _seriesRepository.GetAllAsync()).ToList();

            return new DashboardStatsResponseDto
            {
                Role = MapRoleForFe(roleName),
                PendingSeries = series.Count(s => IsPendingBoardVote(s.Status)),
                ApprovedSeries = series.Count(s => IsBoardApproved(s.Status)),
                InProductionSeries = series.Count(s => IsInProduction(s.Status)),
                Series = series.Count
            };
        }

        private async Task<DashboardStatsResponseDto> BuildEditorStatsAsync(int userId, string roleName)
        {
            var assigned = (await _seriesRepository.FindAsync(s => s.EditorId == userId)).ToList();

            return new DashboardStatsResponseDto
            {
                Role = MapRoleForFe(roleName),
                AssignedSeries = assigned.Count,
                SeriesAwaitingReview = assigned.Count(s => IsPendingApproval(s.Status)),
                PendingSeries = assigned.Count(s => IsPendingApproval(s.Status)),
                InProductionSeries = assigned.Count(s => IsInProduction(s.Status))
            };
        }

        private async Task<DashboardStatsResponseDto> BuildMangakaStatsAsync(int userId, string roleName)
        {
            var mySeries = (await _seriesRepository.FindAsync(s => s.MangakaId == userId)).ToList();
            var tasks = (await _tasksRepository.FindAsync(t => t.MangakaId == userId)).ToList();
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);

            return new DashboardStatsResponseDto
            {
                Role = MapRoleForFe(roleName),
                MySeries = mySeries.Count,
                OpenTasks = tasks.Count(t => t.Status is "Pending" or "In-Progress" or "In_Progress"),
                SetupFundBalance = wallet?.SetupFundBalance ?? 0,
                WithdrawableBalance = wallet?.WithdrawableBalance ?? 0,
                InProductionSeries = mySeries.Count(s => IsInProduction(s.Status))
            };
        }

        private static bool IsPendingApproval(string status)
        {
            return status.Equals("Pending_Approval", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPendingBoardVote(string status)
        {
            return status.Equals("Pending_Board_Vote", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBoardApproved(string status)
        {
            return status.Equals("Fund_Pending", StringComparison.OrdinalIgnoreCase) 
                || status.Equals("Approved", StringComparison.OrdinalIgnoreCase) 
                || status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsInProduction(string status)
        {
            return status.Equals("In Production", StringComparison.OrdinalIgnoreCase)
                || status.Equals("In_Production", StringComparison.OrdinalIgnoreCase);
        }

        private static string MapRoleForFe(string roleName)
        {
            return roleName switch
            {
                "Tantou Editor" => "Editor",
                "Editorial Board" => "Board",
                _ => roleName
            };
        }
    }
}
