using System.ComponentModel.DataAnnotations;

namespace MangaPublishingSystem.Application.DTOs;

public class VnpayCallbackDto
{
    [Required]
    public string Vnp_TxnRef { get; set; } = string.Empty; // reference code from VNPay

    [Required]
    public string Vnp_ResponseCode { get; set; } = string.Empty; // "00" means success
    // Additional VNPay fields can be added as needed
}
