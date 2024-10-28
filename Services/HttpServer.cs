using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModularHttpServer.Utilities;

public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly IRequestHandler _requestHandler;
    private readonly ILogger<HttpServer> _logger;

    public HttpServer(string[] prefixes, IRequestHandler requestHandler, ILogger<HttpServer> logger)
    {
        _listener = new HttpListener();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        foreach (var prefix in prefixes)
        {
            if (!prefix.EndsWith("/"))
            {
                throw new ArgumentException(ErrorCodes.GetErrorMessage(ErrorCodes.URIPrefixNotEndingInSlash));
            }
            _listener.Prefixes.Add(prefix);
            _logger.LogInformation("Added prefix: {0}", prefix);
        }
        _requestHandler = requestHandler;
    }

    public async Task StartAsync()
    {
        try
        {
            _listener.Start();
            _logger.LogInformation("Server started and listening for connections on the following prefixes:");
            foreach (var prefix in _listener.Prefixes)
            {
                _logger.LogInformation(prefix);
            }

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
                    _logger.LogError(ex, ErrorCodes.GetErrorMessage(ErrorCodes.ServerError));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start the server.");
        }
    }

    public void Stop()
    {
        _listener.Stop();
        _logger.LogInformation("Server stopped.");
    }
}
