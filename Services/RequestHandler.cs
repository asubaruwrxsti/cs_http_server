using System.Net;
using System.Text;
using System.Threading.Tasks;

public class RequestHandler : IRequestHandler
{
    public async Task HandleRequestAsync(HttpListenerContext context)
    {
        var response = context.Response;
        string responseString = "<html><body>Hello, world!</body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}