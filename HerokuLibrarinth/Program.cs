using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

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

			Console.WriteLine("Create Program at " + port);
			listener.Prefixes.Add("http://*:" + port + '/');
		}

		protected override void Listen(HttpListenerContext context)
		{
			Console.WriteLine("Create Pusher");
			using(var pusher = new Pusher(context
				,(req,resp) => {
					resp.ContentType	= "text/plain";
					resp.KeepAlive	= true;
				},null))
			{
				int start	= Environment.TickCount;
				int last	= start;

				Console.WriteLine("Pusher Start at " + start);
				bool isAlive	= true;
				while(isAlive)
				{
					Thread.Sleep(100);
					if(Environment.TickCount - last > 3000)
					{
						last	= Environment.TickCount;
						Console.WriteLine("Pusher write at : " + Environment.TickCount);
						pusher.Write(Encoding.Default.GetBytes("{ Time = " + Environment.TickCount + " }"));
					}

					if(Environment.TickCount - start > 15000)
					{
						Console.WriteLine("Pusher dead : " + Environment.TickCount);
						isAlive	= false;
					}
				}
			}
		}

		static void Main(string[] args)
		{
			Run<Program>();
		}
	}
}
