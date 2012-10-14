using System;
using System.IO;
using System.Net;
using System.Threading;

using System.Security.Principal;

namespace Heroku
{
	struct Pusher
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
		readonly Action<byte[],int> callBack;
		public Pusher(HttpListenerContext context,Action<HttpListenerRequest,HttpListenerResponse> prepare,Action<byte[],int> listenCallBack)
		{
			callBack	= listenCallBack;

			User	= context.User;
			request	= context.Request;
			response	= context.Response;

			if(request.Headers["X-FORWARDED-PROTO"] != Uri.UriSchemeHttps)
			{
				var builder	= new UriBuilder(request.Url) { Scheme	= Uri.UriSchemeHttps };
				var uriComponentsWithoutPort	= UriComponents.AbsoluteUri & ~UriComponents.Port;

				response.RedirectLocation	= builder.Uri.GetComponents(uriComponentsWithoutPort,UriFormat.Unescaped);
				response.StatusCode	= (int)HttpStatusCode.Moved;
				response.Close();
			}
			else prepare(request,response);
		}
	}
}
