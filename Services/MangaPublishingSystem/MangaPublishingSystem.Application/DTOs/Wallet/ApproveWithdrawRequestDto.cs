namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class ApproveWithdrawRequestDto
    {
        /// <summary>
        /// Id của giao dịch rút tiền cần duyệt.
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// True = Admin xác nhận đã chuyển khoản thành công. False = Admin từ chối yêu cầu rút tiền.
        /// </summary>
        public bool IsApproved { get; set; }

        /// <summary>
        /// Ghi chú của Admin (lý do từ chối hoặc thông tin chuyển khoản).
        /// </summary>
        public string? AdminNote { get; set; }
    }
}
