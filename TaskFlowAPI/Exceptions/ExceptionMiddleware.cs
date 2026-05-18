using System.Net;
using System.Text.Json;
using TaskFlowAPI.Exceptions;

namespace TaskFlowAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass the request down the pipeline
                // If nothing throws, this middleware does nothing
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                // Known exception type — log as warning, not error
                _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                // Unknown exception — log as error with full details
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

                // In production, hide internal details from the client
                var message = _environment.IsDevelopment()
                    ? ex.Message
                    : "An unexpected error occurred. Please try again later.";

                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, message);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            // Build a ProblemDetails-compatible response
            var problemDetails = new
            {
                type = $"https://tools.ietf.org/html/rfc7231#section-6.{(int)statusCode}",
                title = statusCode.ToString(),
                status = (int)statusCode,
                detail = message,
                traceId = context.TraceIdentifier
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
        }
    }
}