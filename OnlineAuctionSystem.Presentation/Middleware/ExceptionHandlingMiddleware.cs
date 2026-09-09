using OnlineAuctionSystem.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace OnlineAuctionSystem.Presentation.Middleware
{

    // Catches exceptions thrown by MediatR handlers (Application layer) and maps
    // them to the right HTTP status code, so controllers stay free of try/catch.
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode;
            string message;
            IDictionary<string, string[]>? errors = null;

            switch (exception)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;
                case ForbiddenException:
                    statusCode = HttpStatusCode.Forbidden;
                    message = exception.Message;
                    break;
                case UnauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = exception.Message;
                    break;
                case ConflictException:
                    statusCode = HttpStatusCode.Conflict;
                    message = exception.Message;
                    break;
                case AuctionClosedException:
                case InvalidBidException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;
                case FluentValidation.ValidationException validationException:
                    // Was previously falling through to the default 500 case,
                    // hiding real field-level validation errors from the client.
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Validation failed.";
                    errors = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "An unexpected error occurred.";
                    break;
            }

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred.");
            }

            context.Response.StatusCode = (int)statusCode;

            var payload = new
            {
                status = (int)statusCode,
                message,
                errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
