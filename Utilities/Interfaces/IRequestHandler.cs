using System.Net;
using System.Threading.Tasks;

public interface IRequestHandler
{
    Task HandleRequestAsync(HttpListenerContext context);
}