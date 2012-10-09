using System;
using System.IO;
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

			var response	= context.Response;
			var writer	= new StreamWriter(response.OutputStream);
			writer.WriteLine("TestTest");
			writer.Close();
		}

		Program()
		{
			var port	= Environment.GetEnvironmentVariable("PORT");
			if(string.IsNullOrEmpty(port))
				port	= "8888";
			listener.Prefixes.Add("http://+:" + port + '/');

			listener.Start();

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
			while(program.IsAlive)
				Thread.Sleep(100);
		}
	}
}
