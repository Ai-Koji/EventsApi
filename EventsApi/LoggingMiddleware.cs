using System.Diagnostics;

namespace EventsApi
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware>
        logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Request: {Method} {Path}"
            ,
            context.Request.Method,
            context.Request.Path);
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}"
                ,
                context.Request.Method, context.Request.Path);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Response: {StatusCode} for {Method} {Path} in { Elapsed} ms", context.Response.StatusCode, context.Request.Method, context.Request.Path, stopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}
