using System.ComponentModel.DataAnnotations;

namespace MangaPublishingSystem.Application.DTOs
{
    public class DepositRequestDto
    {
        [Required]
        public int WalletId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        // Optional callback URL for VNPay async notification
        public string? CallbackUrl { get; set; }
    }
}
