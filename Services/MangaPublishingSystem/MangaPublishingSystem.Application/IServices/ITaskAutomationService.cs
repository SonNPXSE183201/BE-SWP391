using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface ITaskAutomationService
    {
        Task AutoRefundOverdueTasksAsync();
        Task AutoApproveSubmittedTasksAsync();
        Task CleanExpiredRefreshTokensAsync();
        Task AutoResolveExpiredBoardVotesAsync();
    }
}
