using System;
using System.ComponentModel.DataAnnotations;

namespace MangaPublishingSystem.Application.DTOs.Transaction
{
    /// <summary>
    /// DTO cho yêu cầu truy vấn trạng thái giao dịch VNPay (command: querydr).
    /// </summary>
    public class VnpayQueryRequestDto
    {
        [Required]
        public string Vnp_RequestId { get; set; } = default!; // mã request duy nhất

        [Required]
        public string Vnp_Version { get; set; } = "2.1.0";

        [Required]
        public string Vnp_Command { get; set; } = "querydr";

        [Required]
        public string Vnp_TmnCode { get; set; } = default!; // lấy từ cấu hình

        [Required]
        public string Vnp_TxnRef { get; set; } = default!; // mã tham chiếu giao dịch nội bộ

        public string? OrderInfo { get; set; }
        public string? TransactionDate { get; set; }
        public string? CreateDate { get; set; }
        public string? IpAddr { get; set; }
    }
}
