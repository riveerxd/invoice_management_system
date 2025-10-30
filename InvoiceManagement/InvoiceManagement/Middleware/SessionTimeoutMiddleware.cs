using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace InvoiceManagement.Middleware;

public class SessionTimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionTimeoutMiddleware> _logger;

    public SessionTimeoutMiddleware(RequestDelegate next, ILogger<SessionTimeoutMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if session is available and user is authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Touch the session to extend the sliding expiration
            context.Session.SetString("LastActivity", DateTime.UtcNow.ToString("O"));
        }

        await _next(context);
    }
}

public static class SessionTimeoutMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionTimeout(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionTimeoutMiddleware>();
    }
}
