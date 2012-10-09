using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HerokuLibrarinth
{
	class Program
	{
		IAsyncResult asyncResult;
		readonly HttpListener listener	= new HttpListener();
		void Listen(IAsyncResult result)
		{
			var context	= listener.EndGetContext(result);
			asyncResult	= listener.BeginGetContext(Listen,null);


		}

		Program()
		{
			var port	= Environment.GetEnvironmentVariable("PORT");
			listener.Prefixes.Add("https://*:" + port);
			listener.Prefixes.Add("http://*:" + port);

			listener.Start();

			listener.GetContext();

			asyncResult	= listener.BeginGetContext(Listen,listener);
		}

		static Program()
		{
			if(!HttpListener.IsSupported)
				throw new NotSupportedException(typeof(HttpListener).ToString());
		}

		bool IsAlive
		{
			get { return Console.ReadLine() != "quit"; }
		}

		static void Main(string[] args)
		{
			var program	= new Program();
			var line	= string.Empty;
			while(program.IsAlive)
				Thread.Sleep(100);
		}
	}
}
