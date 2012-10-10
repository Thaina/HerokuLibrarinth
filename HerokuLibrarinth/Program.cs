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

		string state	= null;
		protected override void Listen(HttpListenerContext context)
		{
			var request	= context.Request;
			var response	= context.Response;

			if(request.Headers["X-FORWARDED-PROTO"] != Uri.UriSchemeHttps)
			{
				var builder	= new UriBuilder(request.Url) { Scheme	= Uri.UriSchemeHttps };

				var uriComponentsWithoutPort	= UriComponents.AbsoluteUri & ~UriComponents.Port;

				response.RedirectLocation	= builder.Uri.GetComponents(uriComponentsWithoutPort,UriFormat.Unescaped);
				response.StatusCode	= (int)HttpStatusCode.Moved;
				response.Close();
				return;
			}

			var writer	= new StreamWriter(response.OutputStream);
			writer.WriteLine("This is C# Application");
			writer.WriteLine("Request from " + request.Headers["X-FORWARDED-PROTO"]);
			writer.WriteLine("Last State : " + state);
			writer.WriteLine("New State : " + request.QueryString["state"]);
			writer.Close();

			state	= request.QueryString["state"];
		}

		static void Main(string[] args)
		{
			Run<Program>();
		}
	}
}
