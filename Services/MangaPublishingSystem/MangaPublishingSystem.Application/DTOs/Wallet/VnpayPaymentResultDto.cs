namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    /// <summary>
    /// Kết quả thanh toán trả về cho client sau khi VNPay redirect về ReturnUrl.
    /// </summary>
    public class VnpayPaymentResultDto
    {
        /// <summary>Giao dịch thành công hay không.</summary>
        public bool Success { get; set; }

        /// <summary>Mã tham chiếu giao dịch nội bộ (vnp_TxnRef).</summary>
        public string ReferenceCode { get; set; } = string.Empty;

        /// <summary>Mã giao dịch tại VNPay (vnp_TransactionNo).</summary>
        public string? VnpayTransactionNo { get; set; }

        /// <summary>Số tiền thanh toán (VND, đã chia 100).</summary>
        public decimal Amount { get; set; }

        /// <summary>Mã ngân hàng (vnp_BankCode).</summary>
        public string? BankCode { get; set; }

        /// <summary>Loại thẻ ATM/QR (vnp_CardType).</summary>
        public string? CardType { get; set; }

        /// <summary>Thời gian thanh toán (vnp_PayDate: yyyyMMddHHmmss).</summary>
        public string? PayDate { get; set; }

        /// <summary>Mã phản hồi từ VNPay (vnp_ResponseCode). "00" là thành công.</summary>
        public string? ResponseCode { get; set; }

        /// <summary>Mã trạng thái giao dịch VNPay (vnp_TransactionStatus). "00" là thành công.</summary>
        public string? TransactionStatus { get; set; }

        /// <summary>Nội dung đơn hàng (vnp_OrderInfo).</summary>
        public string? OrderInfo { get; set; }

        /// <summary>Thông báo kết quả hiển thị cho người dùng.</summary>
        public string Message { get; set; } = string.Empty;
    }
}
