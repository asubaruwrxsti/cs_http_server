// Program.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ModularHttpServer.Services;
using ModularHttpServer.Utilities;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IRequestHandler, Router>()
            .AddLogging(configure => configure.AddConsole())
            .AddSingleton<IConfiguration>(configuration)
            .BuildServiceProvider();

        var logger = serviceProvider.GetService<ILogger<HttpServer>>();
        if (logger == null)
        {
            throw new InvalidOperationException(ErrorCodes.GetErrorMessage(ErrorCodes.LoggerServiceNotFound));
        }

        var router = serviceProvider.GetService<IRequestHandler>() as Router;
        if (router == null)
        {
            throw new InvalidOperationException(ErrorCodes.GetErrorMessage(ErrorCodes.RequestHandlerServiceNotFound));
        }

        var url = configuration["HttpServer:Url"];
        if (string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException(ErrorCodes.GetErrorMessage(ErrorCodes.ServerUrlNotConfigured));
        }

        var server = new HttpServer(new[] { url }, router, logger);

        await server.StartAsync();
    }
}