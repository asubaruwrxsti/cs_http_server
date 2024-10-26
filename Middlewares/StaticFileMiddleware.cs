using System;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using System.Net;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ModularHttpServer.Middlewares
{
    public class StaticFiles
    {
        private readonly string _rootPath;
        private readonly ILogger<StaticFiles> _logger;

        public StaticFiles(string rootPath, ILogger<StaticFiles> logger)
        {
            _rootPath = rootPath;
            _logger = logger;
        }

        public async Task<bool> TryServeStaticFileAsync(HttpListenerContext context)
        {
            var filePath = Path.Combine(_rootPath, context.Request.Url.AbsolutePath.TrimStart('/'));

            if (File.Exists(filePath))
            {
                var response = context.Response;
                var buffer = await File.ReadAllBytesAsync(filePath);

                response.ContentLength64 = buffer.Length;
                response.ContentType = GetContentType(filePath);

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
                return true;
            }

            return false;
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "text/javascript",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}