using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ModularHttpServer.Services
{
    public class MiddlewarePipeline
    {
        private readonly List<Func<HttpMiddlewareDelegate, HttpMiddlewareDelegate>> _components;

        public MiddlewarePipeline()
        {
            _components = new List<Func<HttpMiddlewareDelegate, HttpMiddlewareDelegate>>();
        }

        public void Use(Func<HttpMiddlewareDelegate, HttpMiddlewareDelegate> middleware)
        {
            _components.Add(middleware);
        }

        public HttpMiddlewareDelegate Build()
        {
            HttpMiddlewareDelegate next = context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Task.CompletedTask;
            };

            for (int i = _components.Count - 1; i >= 0; i--)
            {
                var component = _components[i];
                next = component(next);
            }

            return next;
        }
    }
}