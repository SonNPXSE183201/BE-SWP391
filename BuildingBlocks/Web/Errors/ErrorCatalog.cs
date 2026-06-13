using System.Net;
using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Errors
{
    public sealed record ErrorDescriptor(int StatusCode, string Message);

    public static class ErrorCatalog
    {
        public static ErrorDescriptor FromStatusCode(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => new ErrorDescriptor(StatusCodes.Status400BadRequest, "Yêu cầu không hợp lệ."),
                StatusCodes.Status401Unauthorized => new ErrorDescriptor(StatusCodes.Status401Unauthorized, "Tài khoản chưa được xác thực hoặc mã token truy cập không hợp lệ."),
                StatusCodes.Status403Forbidden => new ErrorDescriptor(StatusCodes.Status403Forbidden, "Bạn không có quyền truy cập vào tài nguyên này."),
                StatusCodes.Status404NotFound => new ErrorDescriptor(StatusCodes.Status404NotFound, "Không tìm thấy tài nguyên hoặc điểm cuối (endpoint) được yêu cầu."),
                StatusCodes.Status409Conflict => new ErrorDescriptor(StatusCodes.Status409Conflict, "Yêu cầu không thể hoàn thành do có xung đột dữ liệu."),
                StatusCodes.Status500InternalServerError => new ErrorDescriptor(StatusCodes.Status500InternalServerError, "Lỗi hệ thống nội bộ. Vui lòng thử lại sau."),
                _ => new ErrorDescriptor(statusCode, "Yêu cầu thất bại.")
            };
        }

        public static ErrorDescriptor FromException(Exception exception)
        {
            if (exception is CustomException customException)
            {
                return new ErrorDescriptor((int)customException.StatusCode, customException.Message);
            }

            if (exception is NotImplementedException)
            {
                return new ErrorDescriptor((int)HttpStatusCode.NotImplemented, "Tính năng này chưa được phát triển.");
            }

            if (IsDatabaseConnectionIssue(exception))
            {
                return new ErrorDescriptor(StatusCodes.Status500InternalServerError,
                    "Không thể kết nối đến máy chủ cơ sở dữ liệu. Vui lòng kiểm tra kết nối mạng hoặc trạng thái máy chủ cơ sở dữ liệu.");
            }

            return FromStatusCode(StatusCodes.Status500InternalServerError);
        }

        private static bool IsDatabaseConnectionIssue(Exception exception)
        {
            var message = exception.Message;
            return message.Contains("EnableRetryOnFailure")
                   || message.Contains("establishing a connection to SQL Server")
                   || message.Contains("network-related");
        }
    }
}
