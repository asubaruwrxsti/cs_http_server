using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using System.Net;
using Microsoft.Extensions.Logging;
public class StaticFiles
{
    private readonly HttpMiddlewareDelegate _next;
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<StaticFiles> _logger;

    public StaticFiles(HttpMiddlewareDelegate next, string rootPath, ILogger<StaticFiles> logger)
    {
        _next = next;
        _fileProvider = new PhysicalFileProvider(rootPath);
        _logger = logger;
    }

    public async Task InvokeAsync(HttpListenerContext context)
    {
        var filePath = context.Request.Url.AbsolutePath.TrimStart('/');
        _logger.LogInformation($"Attempting to serve file: {filePath}");
        var fileInfo = _fileProvider.GetFileInfo(filePath);

        if (fileInfo.Exists)
        {
            _logger.LogInformation($"Serving file: {filePath}");
            context.Response.ContentType = "text/html";
            using (var stream = fileInfo.CreateReadStream())
            {
                await stream.CopyToAsync(context.Response.OutputStream);
            }
        }
        else
        {
            _logger.LogWarning($"File not found: {filePath}");
            await _next(context);
        }
    }
}