using System.Net;
using System.Threading.Tasks;

public interface IRouteHandler
{
    Task HandleRequestAsync(HttpListenerContext context);
}