using System.Net;

namespace BuildingBlocks.Exceptions
{
    public abstract class CustomException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public Dictionary<string, string[]>? Errors { get; }

        protected CustomException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, Dictionary<string, string[]>? errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }

    // 400 Bad Request
    public class BadRequestException : CustomException
    {
        public BadRequestException(string message, Dictionary<string, string[]>? errors = null)
            : base(message, HttpStatusCode.BadRequest, errors) { }
    }

    // 401 Unauthorized
    public class UnauthorizedException : CustomException
    {
        public UnauthorizedException(string message)
            : base(message, HttpStatusCode.Unauthorized) { }
    }

    // 403 Forbidden
    public class ForbiddenException : CustomException
    {
        public ForbiddenException(string message)
            : base(message, HttpStatusCode.Forbidden) { }
    }

    // 404 Not Found
    public class NotFoundException : CustomException
    {
        public NotFoundException(string message)
            : base(message, HttpStatusCode.NotFound) { }
    }

    // 406 Not Acceptable
    public class NotAcceptableException : CustomException
    {
        public NotAcceptableException(string message)
            : base(message, HttpStatusCode.NotAcceptable) { }
    }

    // 408 Request Timeout
    public class RequestTimeoutException : CustomException
    {
        public RequestTimeoutException(string message)
            : base(message, HttpStatusCode.RequestTimeout) { }
    }

    // 409 Conflict
    public class ConflictException : CustomException
    {
        public ConflictException(string message)
            : base(message, HttpStatusCode.Conflict) { }
    }

    // 413 Payload Too Large
    public class PayloadTooLargeException : CustomException
    {
        public PayloadTooLargeException(string message)
            : base(message, HttpStatusCode.RequestEntityTooLarge) { }
    }

    // 415 Unsupported Media Type
    public class UnsupportedMediaTypeException : CustomException
    {
        public UnsupportedMediaTypeException(string message)
            : base(message, HttpStatusCode.UnsupportedMediaType) { }
    }

    // 422 Unprocessable Entity
    public class UnprocessableEntityException : CustomException
    {
        public UnprocessableEntityException(string message, Dictionary<string, string[]>? errors = null)
            : base(message, HttpStatusCode.UnprocessableEntity, errors) { }
    }

    // 423 Locked
    public class LockedException : CustomException
    {
        public LockedException(string message)
            : base(message, (HttpStatusCode)423) { } // 423 is not in standard HttpStatusCode enum
    }

    // 429 Too Many Requests
    public class TooManyRequestsException : CustomException
    {
        public TooManyRequestsException(string message)
            : base(message, HttpStatusCode.TooManyRequests) { }
    }

    // 500 Internal Server Error
    public class InternalServerException : CustomException
    {
        public InternalServerException(string message, Dictionary<string, string[]>? errors = null)
            : base(message, HttpStatusCode.InternalServerError, errors) { }
    }

    // 502 Bad Gateway
    public class BadGatewayException : CustomException
    {
        public BadGatewayException(string message)
            : base(message, HttpStatusCode.BadGateway) { }
    }

    // 503 Service Unavailable
    public class ServiceUnavailableException : CustomException
    {
        public ServiceUnavailableException(string message)
            : base(message, HttpStatusCode.ServiceUnavailable) { }
    }

    // 504 Gateway Timeout
    public class GatewayTimeoutException : CustomException
    {
        public GatewayTimeoutException(string message)
            : base(message, HttpStatusCode.GatewayTimeout) { }
    }
}
