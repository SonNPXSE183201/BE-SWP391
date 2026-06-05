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
                StatusCodes.Status400BadRequest => new ErrorDescriptor(StatusCodes.Status400BadRequest, "The request is invalid."),
                StatusCodes.Status401Unauthorized => new ErrorDescriptor(StatusCodes.Status401Unauthorized, "You are not authenticated or the access token is invalid."),
                StatusCodes.Status403Forbidden => new ErrorDescriptor(StatusCodes.Status403Forbidden, "You do not have permission to access this resource."),
                StatusCodes.Status404NotFound => new ErrorDescriptor(StatusCodes.Status404NotFound, "The requested resource or endpoint was not found."),
                StatusCodes.Status409Conflict => new ErrorDescriptor(StatusCodes.Status409Conflict, "The request could not be completed because of a conflict."),
                StatusCodes.Status500InternalServerError => new ErrorDescriptor(StatusCodes.Status500InternalServerError, "Internal server error. Please try again later."),
                _ => new ErrorDescriptor(statusCode, "Request failed.")
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
                return new ErrorDescriptor((int)HttpStatusCode.NotImplemented, "This feature is not implemented yet.");
            }

            if (IsDatabaseConnectionIssue(exception))
            {
                return new ErrorDescriptor(StatusCodes.Status500InternalServerError,
                    "Unable to connect to the database server. Please check network connectivity or the database server status.");
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
