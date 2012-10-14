using System;
using System.IO;
using System.Net;
using System.Text;

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
			using(var pusher = new Pusher(context,null))
			{
				int lastTime	= Environment.TickCount;

				bool isAlive	= true;
				while(isAlive)
				{
					if(Environment.TickCount - lastTime > 1000)
						pusher.Write(Encoding.Unicode.GetBytes("{ Time = " + Environment.TickCount + " }"));

					if(Environment.TickCount - lastTime > 60000)
						isAlive	= false;
				}
			}
		}

		static void Main(string[] args)
		{
			Run<Program>();
		}
	}
}
