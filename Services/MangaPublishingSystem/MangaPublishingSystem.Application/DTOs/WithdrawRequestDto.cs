using System.ComponentModel.DataAnnotations;

namespace MangaPublishingSystem.Application.DTOs
{
    public class WithdrawRequestDto
    {
        [Required]
        public int WalletId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
