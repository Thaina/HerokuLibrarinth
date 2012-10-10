using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Heroku
{
	class Program : HerokuBase.Program
	{
		IAsyncResult asyncResult;
		readonly HttpListener listener	= new HttpListener();
		void Listen(IAsyncResult result)
		{
			var context	= listener.EndGetContext(result);
			asyncResult	= listener.BeginGetContext(Listen,null);

			var response	= context.Response;
			var writer	= new StreamWriter(response.OutputStream);
			writer.WriteLine("TestTest");
			writer.Close();
		}

		protected override bool IsAlive
		{
			get { return Console.ReadLine() != "quit"; }
		}

		protected override void InitListener(HttpListener listener)
		{
			var port	= Environment.GetEnvironmentVariable("PORT");
			if(string.IsNullOrEmpty(port))
				port	= "8888";

			listener.Prefixes.Add("http://+:" + port + '/');
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
