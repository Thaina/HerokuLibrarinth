using System;
using System.IO;
using System.Net;
using System.Threading;

using System.Security.Principal;

namespace Heroku
{
	struct Pusher : IDisposable
	{
		public void Write(byte[] buffer,int offset = 0,int count = 0)
		{
			if(count < 1)
				count	= buffer.Length;
			response.OutputStream.Write(buffer,offset,count);
			response.OutputStream.Flush();
			Thread.Sleep(1);
		}

		public readonly IPrincipal User;
		readonly HttpListenerRequest request;
		readonly HttpListenerResponse response;
		public Pusher(HttpListenerContext context,Action<HttpListenerRequest,HttpListenerResponse> prepare)
		{
			User	= context.User;
			request	= context.Request;
			response	= context.Response;
			if(request.Url.Port != 8888 && request.Headers["X-FORWARDED-PROTO"] != Uri.UriSchemeHttps)
			{
				var builder	= new UriBuilder(request.Url) { Scheme	= Uri.UriSchemeHttps };
				var uriComponentsWithoutPort	= UriComponents.AbsoluteUri & ~UriComponents.Port;

				response.RedirectLocation	= builder.Uri.GetComponents(uriComponentsWithoutPort,UriFormat.Unescaped);
				response.StatusCode	= (int)HttpStatusCode.Moved;
			}
			else prepare(request,response);
		}

		public void Dispose()
		{
			response.OutputStream.Close();
			response.Close();
		}
	}
}
