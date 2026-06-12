using Microsoft.AspNetCore.Http;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IVnPayService
    {
        /// <summary>
        /// Tạo URL thanh toán VNPay Sandbox có chữ ký HMAC-SHA512.
        /// </summary>
        /// <param name="referenceCode">Mã tham chiếu giao dịch nội bộ (vnp_TxnRef).</param>
        /// <param name="amount">Số tiền VND (sẽ được nhân 100 trước khi gửi sang VNPay).</param>
        /// <param name="ipAddr">Địa chỉ IP của người dùng.</param>
        /// <param name="orderInfo">Mô tả nội dung thanh toán.</param>
        /// <returns>URL đầy đủ có chữ ký để redirect người dùng đến VNPay.</returns>
        string BuildPaymentUrl(string referenceCode, decimal amount, string ipAddr, string orderInfo);

        /// <summary>
        /// Xác minh chữ ký HMAC-SHA512 từ callback/IPN của VNPay.
        /// </summary>
        /// <param name="queryParams">Toàn bộ query string từ request VNPay gọi về.</param>
        /// <param name="referenceCode">Mã tham chiếu giao dịch (vnp_TxnRef) nếu hợp lệ.</param>
        /// <param name="isSuccess">True nếu vnp_ResponseCode == "00" và chữ ký hợp lệ.</param>
        /// <returns>True nếu chữ ký hợp lệ.</returns>
        bool ValidateCallback(IQueryCollection queryParams, out string referenceCode, out bool isSuccess);
    }
}
