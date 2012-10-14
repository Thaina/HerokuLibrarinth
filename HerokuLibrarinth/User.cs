using System;
using System.IO;
using System.Net;
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
		}

		readonly byte[] buffer;
		public readonly IPrincipal User;
		readonly HttpListenerRequest request;
		readonly HttpListenerResponse response;
		readonly Action<byte[],int> callBack;
		public Pusher(HttpListenerContext context,Action<byte[],int> listenCallBack)
		{
			callBack	= listenCallBack;

			User	= context.User;
			request	= context.Request;
			response	= context.Response;

			asyncResult	= null;
			if(request.Headers["X-FORWARDED-PROTO"] != Uri.UriSchemeHttps)
			{
				buffer	= null;

				var builder	= new UriBuilder(request.Url) { Scheme	= Uri.UriSchemeHttps };

				var uriComponentsWithoutPort	= UriComponents.AbsoluteUri & ~UriComponents.Port;

				response.RedirectLocation	= builder.Uri.GetComponents(uriComponentsWithoutPort,UriFormat.Unescaped);
				response.StatusCode	= (int)HttpStatusCode.Moved;
				response.Close();
			}
			else
			{
				buffer	= new byte[256];
				BeginRead();
			}
		}

		IAsyncResult asyncResult;
		void BeginRead(IAsyncResult result = null)
		{
			if(result != null && callBack != null)
				callBack(buffer,request.InputStream.EndRead(result));
			asyncResult	= request.InputStream.BeginRead(buffer,0,buffer.Length,BeginRead,null);
		}

		public void Dispose()
		{
			asyncResult.AsyncWaitHandle.Close();
			asyncResult.AsyncWaitHandle.Dispose();
		}
	}
}
