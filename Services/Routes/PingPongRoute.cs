using System.Threading.Tasks;
using System.Net;
using ModularHttpServer.Utilities.Attributes;

namespace ModularHttpServer.Services.Routing
{
    [Route("/ping")]
    public class PingPongRouteHandler : IRouteHandler
    {
        public async Task HandleRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;
            var responseString = "Pong";
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/plain";

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
    }
}