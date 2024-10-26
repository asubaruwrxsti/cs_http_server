using System.Net;
using System.Threading.Tasks;

public delegate Task HttpMiddlewareDelegate(HttpListenerContext context);
