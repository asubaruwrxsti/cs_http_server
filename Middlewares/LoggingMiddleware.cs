using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class LoggingMiddleware : IHttpMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpListenerContext context, HttpMiddlewareDelegate next)
    {
        _logger.LogInformation("Handling request: {0}", context.Request.Url);
        await next(context);
        _logger.LogInformation("Finished handling request.");
    }
}