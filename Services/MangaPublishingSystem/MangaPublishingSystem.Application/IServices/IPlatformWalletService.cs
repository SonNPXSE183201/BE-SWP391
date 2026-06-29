using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IPlatformWalletService
    {
        Task<PlatformWalletDto> GetTreasuryAsync();
        /// <summary>Tạo giao dịch Pending và trả URL thanh toán VNPay Sandbox.</summary>
        Task<string> InitiateTopUpTreasuryAsync(int adminUserId, TopUpPlatformWalletDto dto, string ipAddr);
        /// <summary>Trừ ví NXB, cộng SetupFundBalance Mangaka, ghi 2 bút toán đối soát.</summary>
        System.Threading.Tasks.Task DisburseProductionFundAsync(int seriesId, int mangakaId, decimal amount, Wallet mangakaWallet);
    }
}
