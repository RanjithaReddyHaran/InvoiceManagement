using System.Net;
using System.Text.Json;

namespace StaffingInvoice.Api.Middleware;

/// <summary>Converts unhandled exceptions into consistent Problem Details responses.</summary>
public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            var (status, title) = ex switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                ArgumentException => (HttpStatusCode.BadRequest, "Invalid request"),
                InvalidOperationException => (HttpStatusCode.Conflict, "Business rule violation"),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/problem+json";
            var payload = new { type = "about:blank", title, status = (int)status,
                detail = status == HttpStatusCode.InternalServerError ? "Please contact support with the correlation ID." : ex.Message,
                traceId = context.TraceIdentifier };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
