// Services/Routing/Router.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModularHttpServer.Utilities;
using ModularHttpServer.Utilities.Attributes;

namespace ModularHttpServer.Services
{
    public class Router : IRequestHandler
    {
        private readonly Dictionary<string, IRouteHandler> _routes;
        private readonly ILogger<Router> _logger;

        public Router(ILogger<Router> logger)
        {
            _routes = new Dictionary<string, IRouteHandler>();
            _logger = logger;
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
            if (_routes.TryGetValue(path, out var handler))
            {
                await handler.HandleRequestAsync(context);
            }
            else
            {
                _logger.LogWarning(ErrorCodes.GetErrorMessage(ErrorCodes.NoHandlerForPath));
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.Close();
            }
        }
    }
}