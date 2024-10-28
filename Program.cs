using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ModularHttpServer.Services;
using ModularHttpServer.Utilities;
using MiddlewarePipeline = ModularHttpServer.Services.MiddlewarePipeline;
using ModularHttpServer.Middlewares;

class Program
{
    static async Task Main(string[] args)
    {
        while (true)
        {
            try
            {
                using var host = CreateHostBuilder(args).Build();
                var logger = host.Services.GetRequiredService<ILogger<HttpServer>>();
                var server = host.Services.GetRequiredService<HttpServer>();

                await server.StartAsync();
                logger.LogInformation("Server started successfully.");
                await host.RunAsync();
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during server execution: {ex}");
                await Task.Delay(1000);
            }
        }
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
            services.AddSingleton<IRequestHandler, Router>();
            services.AddSingleton<MiddlewarePipeline>();
            services.AddLogging(configure => configure.AddConsole());
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton(sp => new DbConnection(configuration));
            services.AddSingleton(sp =>
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
            services.AddSingleton<LoggingMiddleware>();
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<StaticFiles>>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                var rootPath = configuration["StaticFiles:RootPath"];
                if (string.IsNullOrEmpty(rootPath))
                {
                    logger.LogError("Static files root path is not configured.");
                    throw new InvalidOperationException(ErrorCodes.GetErrorMessage(ErrorCodes.StaticFilesRootNotConfigured));
                }
                rootPath = Path.GetFullPath(rootPath);
                Console.WriteLine($"Static files root path: {rootPath}");
                return new StaticFiles(rootPath, logger);
            });

            // Configure middleware pipeline
            services.AddSingleton(sp =>
            {
                var pipeline = new MiddlewarePipeline();

                // Logging middleware
                pipeline.Use(next => async context =>
                {
                    var loggingMiddleware = sp.GetRequiredService<LoggingMiddleware>();
                    await loggingMiddleware.InvokeAsync(context, next);
                });

                // Static files middleware
                pipeline.Use(next => async context =>
                {
                    var staticFiles = sp.GetRequiredService<StaticFiles>();
                    if (!await staticFiles.TryServeStaticFileAsync(context))
                    {
                        await next(context);
                    }
                });

                // Auth middleware
                pipeline.Use(next => async context =>
                {
                    var authMiddleware = sp.GetRequiredService<AuthMiddleware>();
                    await authMiddleware.InvokeAsync(context, next);
                });

                return pipeline;
            });
        });
    }
}