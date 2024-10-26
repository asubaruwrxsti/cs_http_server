using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModularHttpServer.Middlewares;
using ModularHttpServer.Utilities;
using ModularHttpServer.Utilities.Attributes;
namespace ModularHttpServer.Services
{
    public class Router : IRequestHandler
    {
        private readonly Dictionary<string, IRouteHandler> _routes;
        private readonly ILogger<Router> _logger;
        private readonly MiddlewarePipeline _pipeline;
        private readonly StaticFiles _staticFiles;

        public Router(ILogger<Router> logger, MiddlewarePipeline pipeline, StaticFiles staticFiles)
        {
            _routes = new Dictionary<string, IRouteHandler>();
            _logger = logger;
            _pipeline = pipeline;
            _staticFiles = staticFiles;
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            var routeHandlerTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IRouteHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in routeHandlerTypes)
            {
                var routeAttribute = type.GetCustomAttribute<RouteAttribute>();
                if (routeAttribute != null)
                {
                    var handler = (IRouteHandler)Activator.CreateInstance(type);
                    _routes[routeAttribute.Path] = handler;
                    _logger.LogInformation($"Registered route: {routeAttribute.Path}");
                }
            }
        }

        public async Task HandleRequestAsync(HttpListenerContext context)
        {
            var path = context.Request.Url.AbsolutePath;

            // Check if the path matches a registered route
            if (_routes.TryGetValue(path, out var handler))
            {
                var middlewareDelegate = _pipeline.Build();
                await middlewareDelegate(context);
                await handler.HandleRequestAsync(context);
            }
            else
            {
                // Check if the path corresponds to a static file
                if (await _staticFiles.TryServeStaticFileAsync(context))
                {
                    return;
                }

                // If no route or static file matches, return 404
                _logger.LogWarning(ErrorCodes.GetErrorMessage(ErrorCodes.NoHandlerForPath));
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
            }
        }
    }
}