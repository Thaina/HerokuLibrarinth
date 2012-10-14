using System;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

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

			if(request.Headers["X-FORWARDED-PROTO"] != Uri.UriSchemeHttps)
			{
				buffer	= null;
				isAlive	= false;

				var builder	= new UriBuilder(request.Url) { Scheme	= Uri.UriSchemeHttps };

				var uriComponentsWithoutPort	= UriComponents.AbsoluteUri & ~UriComponents.Port;

				response.RedirectLocation	= builder.Uri.GetComponents(uriComponentsWithoutPort,UriFormat.Unescaped);
				response.StatusCode	= (int)HttpStatusCode.Moved;
				response.Close();
			}
			else
			{
				isAlive	= true;
				buffer	= new byte[256];
				Parallel.Invoke(BeginRead);
			}
		}

		bool isAlive;
		void BeginRead()
		{
			while(isAlive)
			{
				int length	= request.InputStream.Read(buffer,0,buffer.Length);
				if(callBack != null)
					callBack(buffer,length);
			}

			response.OutputStream.Close();
			response.Close();
		}
		
		public void Dispose() { isAlive	= false; }
	}
}
