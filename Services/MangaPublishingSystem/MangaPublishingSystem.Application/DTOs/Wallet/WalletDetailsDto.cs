using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class WalletDetailsDto
    {
        public WalletDto Wallet { get; set; } = null!;
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    }
}
