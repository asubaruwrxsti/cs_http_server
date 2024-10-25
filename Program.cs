using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using ModularHttpServer.Services;
using ModularHttpServer.Utilities;
using MiddlewarePipeline = ModularHttpServer.Services.MiddlewarePipeline; // Alias for MiddlewarePipeline

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var logger = host.Services.GetRequiredService<ILogger<HttpServer>>();
        var router = host.Services.GetRequiredService<IRequestHandler>() as Router;
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        var url = configuration["HttpServer:Url"];
        if (string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException(ErrorCodes.GetErrorMessage(ErrorCodes.ServerUrlNotConfigured));
        }

        var server = host.Services.GetRequiredService<HttpServer>();

        var hotReloadEnabled = configuration.GetValue<bool>("HotReload:Enabled");
        if (hotReloadEnabled)
        {
            logger.LogInformation("Hot reload enabled. Watching for file changes...");
            var fileProvider = new PhysicalFileProvider(AppContext.BaseDirectory);
            var token = fileProvider.Watch("**/*.cs");

            _ = token.RegisterChangeCallback(state =>
            {
                logger.LogInformation("File changes detected. Restarting server...");
                host.StopAsync().Wait();
                host.RunAsync().Wait();
            }, null);
        }
        else
        {
            logger.LogInformation("Hot reload disabled.");
        }

        await server.StartAsync();
        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            _ = services.AddSingleton<IRequestHandler, Router>();
            _ = services.AddSingleton<MiddlewarePipeline>();
            _ = services.AddLogging(configure => configure.AddConsole());
            _ = services.AddSingleton<IConfiguration>(configuration);
            _ = services.AddSingleton<HttpServer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<HttpServer>>();
                var router = sp.GetRequiredService<IRequestHandler>() as Router;
                var url = configuration["HttpServer:Url"];
                if (string.IsNullOrEmpty(url))
                {
                    logger.LogError("Server URL is not configured.");
                    throw new InvalidOperationException(ErrorCodes.GetErrorMessage(ErrorCodes.ServerUrlNotConfigured));
                }
                return new HttpServer([url], router, logger);
            });

            // Configure middleware
            _ = services.AddSingleton<LoggingMiddleware>();
            _ = services.AddSingleton<StaticFiles>(sp =>
            {
                var rootPath = configuration["StaticFiles:RootPath"];
                if (!System.IO.Path.IsPathRooted(rootPath))
                {
                    rootPath = System.IO.Path.Combine(AppContext.BaseDirectory, rootPath);
                }
                var logger = sp.GetRequiredService<ILogger<StaticFiles>>();
                return new StaticFiles(sp.GetRequiredService<MiddlewarePipeline>().Build(), rootPath, logger);
            });

            // Configure pipeline
            _ = services.AddSingleton<MiddlewarePipeline>(sp =>
            {
                var pipeline = new MiddlewarePipeline();
                pipeline.Use(next => async context =>
                {
                    var loggingMiddleware = sp.GetRequiredService<LoggingMiddleware>();
                    await loggingMiddleware.InvokeAsync(context, next);
                });
                // pipeline.Use(next => async context =>
                // {
                //     var staticFiles = sp.GetRequiredService<StaticFiles>();
                //     await staticFiles.InvokeAsync(context);
                // });
                return pipeline;
            });
        });
    }
}