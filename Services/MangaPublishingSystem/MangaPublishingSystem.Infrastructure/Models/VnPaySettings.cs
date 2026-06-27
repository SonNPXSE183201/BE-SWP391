namespace MangaPublishingSystem.Infrastructure.Models
{
    public class VnPaySettings
    {
        /// <summary>Mã website merchant do VNPay cấp (vnp_TmnCode).</summary>
        public string TmnCode { get; set; } = string.Empty;

        /// <summary>Chuỗi bí mật dùng để tạo và xác minh chữ ký HMAC-SHA512 (vnp_HashSecret).</summary>
        public string HashSecret { get; set; } = string.Empty;

        /// <summary>URL cổng thanh toán VNPay Sandbox.</summary>
        public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        /// <summary>URL truy vấn / hoàn tiền giao dịch VNPay Sandbox.</summary>
        public string QueryUrl { get; set; } = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";

        /// <summary>URL hệ thống nhận kết quả redirect từ browser sau khi thanh toán (vnp_ReturnUrl).</summary>
        public string ReturnUrl { get; set; } = string.Empty;

        /// <summary>URL frontend Mangaka/Assistant sau khi nạp ví cá nhân.</summary>
        public string FrontendReturnUrl { get; set; } = "http://localhost:5173/mangaka/wallet";

        /// <summary>URL frontend Admin sau khi nạp quỹ ví NXB qua VNPay.</summary>
        public string AdminFrontendReturnUrl { get; set; } = "http://localhost:5173/admin/reconciliation";

        /// <summary>URL hệ thống nhận IPN server-to-server từ VNPay (phải HTTPS khi production).</summary>
        public string IpnUrl { get; set; } = string.Empty;
    }
}
