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

			listener.Prefixes.Add("https://+:" + port + '/');
		}

		protected override void Listen(HttpListenerContext context)
		{
			var response	= context.Response;
			var writer	= new StreamWriter(response.OutputStream);
			writer.WriteLine("TestTest");
			writer.Close();
		}

		static void Main(string[] args)
		{
			Run<Program>();
		}
	}
}
