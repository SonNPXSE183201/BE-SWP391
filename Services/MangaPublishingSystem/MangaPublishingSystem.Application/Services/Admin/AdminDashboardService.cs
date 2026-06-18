using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Enums;

namespace MangaPublishingSystem.Application.Services.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IContractRepository _contractRepository;

        public AdminDashboardService(
            IUserRepository userRepository,
            ISeriesRepository seriesRepository,
            ITransactionRepository transactionRepository,
            IContractRepository contractRepository)
        {
            _userRepository = userRepository;
            _seriesRepository = seriesRepository;
            _transactionRepository = transactionRepository;
            _contractRepository = contractRepository;
        }

        public async Task<AdminDashboardResponseDto> GetAdminDashboardAsync()
        {
            var users = (await _userRepository.GetAllAsync()).ToList();
            var series = (await _seriesRepository.GetAllAsync()).ToList();
            var transactions = (await _transactionRepository.GetAllAsync()).ToList();
            var contracts = (await _contractRepository.GetAllAsync()).ToList();

            var stats = new AdminDashboardStatsDto
            {
                Users = users.Count,
                Approvals = users.Count(u => u.RoleId == 5 && u.Status == UserStatus.Pending),
                Series = series.Count,
                Transactions = transactions.Count
            };

            var activities = new List<AdminRecentActivityDto>();

            foreach (var user in users.OrderByDescending(u => u.CreateAt).Take(5))
            {
                activities.Add(new AdminRecentActivityDto
                {
                    Id = user.Id.ToString(),
                    Title = $"Người dùng: {user.FullName} ({user.UserName})",
                    Date = FormatDate(user.CreateAt),
                    Type = "user"
                });
            }

            foreach (var contract in contracts.OrderByDescending(c => c.CreateAt).Take(3))
            {
                activities.Add(new AdminRecentActivityDto
                {
                    Id = $"contract-{contract.Id}",
                    Title = $"Hợp đồng #{contract.Id} — Series #{contract.SeriesId}",
                    Date = FormatDate(contract.CreateAt),
                    Type = "contract"
                });
            }

            foreach (var tx in transactions.OrderByDescending(t => t.CreateAt).Take(3))
            {
                activities.Add(new AdminRecentActivityDto
                {
                    Id = $"tx-{tx.Id}",
                    Title = $"Giao dịch {tx.Type}: {tx.Amount:N0} VND",
                    Date = FormatDate(tx.CreateAt),
                    Type = "transaction"
                });
            }

            return new AdminDashboardResponseDto
            {
                Stats = stats,
                RecentActivities = activities
                    .OrderByDescending(a => a.Date)
                    .Take(10)
                    .ToList()
            };
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm");
        }
    }
}
