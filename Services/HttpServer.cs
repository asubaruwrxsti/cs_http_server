using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ModularHttpServer.Services
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly IRequestHandler _requestHandler;
        private readonly ILogger<HttpServer> _logger;

        public HttpServer(string[] prefixes, IRequestHandler requestHandler, ILogger<HttpServer> logger)
        {
            _listener = new HttpListener();
            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _requestHandler = requestHandler;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _logger.LogInformation("Listening for connections...");

            while (true)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _logger.LogInformation("Received request: {0}", context.Request.Url);
                    await _requestHandler.HandleRequestAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the request.");
                }
            }
        }

        public void Stop()
        {
            _listener.Stop();
            _logger.LogInformation("Server stopped.");
        }
    }
}