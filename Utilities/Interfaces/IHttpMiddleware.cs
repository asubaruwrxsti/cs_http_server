using System.Net;
using System.Threading.Tasks;

public interface IHttpMiddleware
{
    Task InvokeAsync(HttpListenerContext context, HttpMiddlewareDelegate next);
}