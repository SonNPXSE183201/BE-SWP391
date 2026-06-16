using System.Net;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Web.Responses
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }

        public ApiResponse() { }

        public ApiResponse(int statusCode, bool isSuccess, string message, T? data = default, Dictionary<string, string[]>? errors = null)
        {
            StatusCode = statusCode;
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            Errors = errors;
        }

        // --- Success Helpers ---

        public static ApiResponse<T> Success(T data, string message = "Success", int statusCode = (int)HttpStatusCode.OK)
        {
            return new ApiResponse<T>(statusCode, true, message, data);
        }

        public static ApiResponse<T> Created(T data, string message = "Created")
        {
            return new ApiResponse<T>((int)HttpStatusCode.Created, true, message, data);
        }

        public static ApiResponse<T> Accepted(T data, string message = "Accepted")
        {
            return new ApiResponse<T>((int)HttpStatusCode.Accepted, true, message, data);
        }

        // Technically 204 No Content doesn't return a body, but for uniformity if returned as JSON:
        public static ApiResponse<T> NoContent(string message = "No Content")
        {
            return new ApiResponse<T>((int)HttpStatusCode.NoContent, true, message, default);
        }

        // --- Redirect/Cache Helpers ---

        public static ApiResponse<T> MovedPermanently(string message = "Moved Permanently")
        {
            return new ApiResponse<T>((int)HttpStatusCode.MovedPermanently, true, message, default);
        }

        public static ApiResponse<T> NotModified(string message = "Not Modified")
        {
            return new ApiResponse<T>((int)HttpStatusCode.NotModified, true, message, default);
        }

        // --- Error Helpers ---
        // These are mostly used by the GlobalExceptionMiddleware to format errors consistently

        public static ApiResponse<T> Failure(int statusCode, string message, Dictionary<string, string[]>? errors = null)
        {
            return new ApiResponse<T>(statusCode, false, message, default, errors);
        }
    }
}
