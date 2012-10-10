using System;
using System.IO;
using System.Net;

namespace Heroku
{
	class Program : HerokuBase.Program
	{
		protected override bool IsAlive
		{
			get { return Console.ReadLine() != "quit"; }
		}

		protected override void InitListener(HttpListener listener)
		{
			var port	= Environment.GetEnvironmentVariable("PORT");
			if(string.IsNullOrEmpty(port))
				port	= "8888";

			listener.Prefixes.Add("http://*:" + port + '/');
		}

		protected override void Listen(HttpListenerContext context)
		{
			var request	= context.Request;
			var response	= context.Response;

			var writer	= new StreamWriter(response.OutputStream);
			writer.WriteLine("This is C# Application");
			writer.WriteLine("Request from " + request.Headers["X-FORWARDED-PROTO"]);

			if(request.Headers["X-FORWARDED-PROTO"] != Uri.UriSchemeHttps)
			{
				var uriComponentsWithoutPort	= UriComponents.AbsoluteUri & ~UriComponents.Port;
				var urlWithoutPort	= request.Url.GetComponents(uriComponentsWithoutPort,UriFormat.Unescaped);
				var builder	= new UriBuilder(urlWithoutPort)
				{
					Scheme	= Uri.UriSchemeHttps
				};

				writer.WriteLine("Not HTTPS. Redirect to : " + builder.Uri.ToString());
			}

			writer.Close();
		}

		static void Main(string[] args)
		{
			Run<Program>();
		}
	}
}
