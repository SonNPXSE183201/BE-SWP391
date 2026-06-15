using System;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Infrastructure.Models;
using MangaPublishingSystem.Infrastructure.VnPay;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MangaPublishingSystem.Infrastructure.Services
{
    /// <summary>
    /// Dịch vụ tích hợp VNPay Sandbox: tạo URL thanh toán và xác minh callback/IPN.
    /// </summary>
    public class VnPayService : IVnPayService
    {
        private readonly VnPaySettings _settings;

        public VnPayService(IOptions<VnPaySettings> settings)
        {
            _settings = settings.Value;
        }

        /// <inheritdoc/>
        public string BuildPaymentUrl(string referenceCode, decimal amount, string ipAddr, string orderInfo)
        {
            var vnpay = new VnPayLibrary();
            var createDate = DateTime.UtcNow.AddHours(7); // UTC+7 cho VNPay

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _settings.TmnCode);

            // VNPay yêu cầu số tiền * 100 (không có phần thập phân)
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());

            vnpay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddr);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _settings.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", referenceCode);

            // Thời hạn thanh toán: 15 phút
            vnpay.AddRequestData("vnp_ExpireDate", createDate.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            return vnpay.CreateRequestUrl(_settings.PaymentUrl, _settings.HashSecret);
        }

        /// <inheritdoc/>
        public bool ValidateCallback(IQueryCollection queryParams, out string referenceCode, out bool isSuccess)
        {
            referenceCode = string.Empty;
            isSuccess = false;

            // Xác minh chữ ký HMAC-SHA512
            if (!VnPayLibrary.ValidateSignature(queryParams, _settings.HashSecret))
                return false;

            // Lấy mã tham chiếu giao dịch nội bộ
            if (queryParams.TryGetValue("vnp_TxnRef", out var txnRef))
                referenceCode = txnRef.ToString();

            // Kiểm tra mã phản hồi VNPay — "00" là thành công
            if (queryParams.TryGetValue("vnp_ResponseCode", out var responseCode))
                isSuccess = responseCode.ToString() == "00";

            return true;
        }
    }
}
