using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MangaPublishingSystem.Infrastructure.VnPay
{
    /// <summary>
    /// Thư viện helper xử lý tạo URL thanh toán và xác minh chữ ký HMAC-SHA512 theo chuẩn VNPay 2.1.0.
    /// </summary>
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";

        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(StringComparer.Ordinal); // VNPay requires Ordinal sorting

        /// <summary>Thêm tham số vào danh sách request (bỏ qua nếu giá trị rỗng).</summary>
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _requestData[key] = value;
            }
        }

        /// <summary>
        /// Tạo URL thanh toán đầy đủ có chữ ký HMAC-SHA512.
        /// </summary>
        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var queryBuilder = new StringBuilder();
            foreach (var kvp in _requestData)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    queryBuilder.Append(System.Net.WebUtility.UrlEncode(kvp.Key))
                                .Append('=')
                                .Append(System.Net.WebUtility.UrlEncode(kvp.Value))
                                .Append('&');
                }
            }

            var hashData = queryBuilder.ToString();
            if (hashData.Length > 0)
                hashData = hashData.Substring(0, hashData.Length - 1); // remove trailing &

            var secureHash = HmacSha512(hashSecret, hashData);

            return $"{baseUrl}?{hashData}&vnp_SecureHash={secureHash}";
        }

        /// <summary>
        /// Xác minh chữ ký HMAC-SHA512 từ request callback/IPN của VNPay.
        /// </summary>
        public static bool ValidateSignature(IQueryCollection queryParams, string hashSecret)
        {
            if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash) || string.IsNullOrWhiteSpace(receivedHash))
                return false;

            // VNPay requires Ordinal sorting for validation too
            var sortedParams = queryParams
                .Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(kvp => $"{System.Net.WebUtility.UrlEncode(kvp.Key)}={System.Net.WebUtility.UrlEncode(kvp.Value.ToString())}");

            var hashData = string.Join("&", sortedParams);
            var computedHash = HmacSha512(hashSecret, hashData);

            return string.Equals(computedHash, receivedHash.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Tính HMAC-SHA512 với key và data cho trước.</summary>
        public static string HmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
