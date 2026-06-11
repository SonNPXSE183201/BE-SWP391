namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    /// <summary>
    /// DTO bind toàn bộ query parameters VNPay gửi về qua ReturnUrl hoặc IPN.
    /// Các property được bind tự động bởi ASP.NET Core model binding theo tên property
    /// (không dùng [FromQuery(Name=...)] để giữ sạch tầng Application).
    /// Controller đọc thêm các giá trị cần thiết trực tiếp từ HttpContext.Request.Query.
    /// </summary>
    public class VnpayReturnDto
    {
        /// <summary>Mã website merchant.</summary>
        public string? vnp_TmnCode { get; set; }

        /// <summary>Số tiền thanh toán (đơn vị: đồng, đã nhân 100).</summary>
        public long vnp_Amount { get; set; }

        /// <summary>Mã ngân hàng (ATM/QR...).</summary>
        public string? vnp_BankCode { get; set; }

        /// <summary>Mã giao dịch tại ngân hàng.</summary>
        public string? vnp_BankTranNo { get; set; }

        /// <summary>Loại thẻ.</summary>
        public string? vnp_CardType { get; set; }

        /// <summary>Nội dung thanh toán.</summary>
        public string? vnp_OrderInfo { get; set; }

        /// <summary>Thời gian thanh toán (yyyyMMddHHmmss).</summary>
        public string? vnp_PayDate { get; set; }

        /// <summary>Mã phản hồi VNPay — "00" là thành công.</summary>
        public string? vnp_ResponseCode { get; set; }

        /// <summary>Mã giao dịch tại VNPay.</summary>
        public string? vnp_TransactionNo { get; set; }

        /// <summary>Trạng thái giao dịch VNPay — "00" là thành công.</summary>
        public string? vnp_TransactionStatus { get; set; }

        /// <summary>Mã tham chiếu giao dịch nội bộ (referenceCode).</summary>
        public string? vnp_TxnRef { get; set; }

        /// <summary>Chữ ký HMAC-SHA512 do VNPay tạo để xác minh tính toàn vẹn dữ liệu.</summary>
        public string? vnp_SecureHash { get; set; }
    }
}
