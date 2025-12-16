using System.Diagnostics;

namespace LibraryManagementSystem.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            _logger.LogInformation("Incoming request: {Method} {Path} from {IP}",
                request.Method, request.Path, context.Connection.RemoteIpAddress);

            await _next(context);

            stopwatch.Stop();
            _logger.LogInformation("Outgoing response: {StatusCode} in {ElapsedMilliseconds}ms",
                context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}