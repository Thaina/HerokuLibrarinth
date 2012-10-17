using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Heroku
{
	struct Asyncer<T>
	{
		Func<T> Func;
		IAsyncResult asyncResult;
		public Asyncer(Func<T> func)
		{
			Func	= func;
			asyncResult	= null;
		}

		public T Result
		{
			get
			{
				if(asyncResult == null)
					asyncResult	= Func.BeginInvoke(null,null);

				if(!asyncResult.IsCompleted)
					return default(T);
				
				var result	= Func.EndInvoke(asyncResult);
				asyncResult	= null;
				return result;
			}
		}
	}

	class Program : HerokuBase.Program,Pusher.ICallBack
	{
		Asyncer<string> consoleReadLine	= new Asyncer<string>(Console.ReadLine);
		protected override bool IsAlive
		{
			get { return consoleReadLine.Result != "quit"; }
		}

		protected override void InitListener(HttpListener listener)
		{
			var port	= Environment.GetEnvironmentVariable("PORT");
			if(string.IsNullOrEmpty(port))
				port	= "8888";

			Console.WriteLine("Create Program at " + port);
			listener.Prefixes.Add("http://*:" + port + '/');
		}


		protected override void Listen(HttpListenerContext context)
		{
			Console.WriteLine("Create Pusher");
			using(var pusher = new Pusher(context,this))
			{
				int start	= Environment.TickCount;
				int last	= start;

				Console.WriteLine("Pusher Start at " + start);
				bool isAlive	= true;
				while(isAlive)
				{
					Thread.Sleep(100);
					if(Environment.TickCount - last > 1000)
					{
						last	= Environment.TickCount;
						Console.WriteLine("Pusher write at : " + Environment.TickCount);
						pusher.Write(Encoding.UTF8.GetBytes("{ Time = " + Environment.TickCount + " }\n"));
					}

					if(Environment.TickCount - start > 15000)
					{
						Console.WriteLine("Pusher dead : " + Environment.TickCount);
						isAlive	= false;
					}
				}
			}

		}

		protected override void Work()
		{
		}

		public void Prepare(HttpListenerRequest request,HttpListenerResponse response)
		{
			response.SendChunked	= true;
			response.ContentEncoding	= Encoding.UTF8;
			response.ContentType	= "text/json; charset=UTF-8";
			response.AddHeader("Access-Control-Allow-Origin","*");
			response.AddHeader("Cache-Control","no-cache");
		}

		public void Disconnected(IPrincipal user,HttpListenerRequest request,HttpListenerResponse response)
		{
		}

		static void Main(string[] args)
		{
			Run<Program>();
		}
	}
}
