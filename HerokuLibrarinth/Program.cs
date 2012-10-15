using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;

namespace Heroku
{
	class Program : HerokuBase.Program
	{
		protected override bool IsAlive
		{
			get
			{
				Func<string> action	= Console.ReadLine;
				var result	= action.BeginInvoke(null,null);
				result.AsyncWaitHandle.WaitOne(10000);
				return !result.IsCompleted || action.EndInvoke(result) != "quit";
			}
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
				,(req,resp) =>
				{
					resp.SendChunked	= true;
					resp.ContentEncoding	= Encoding.UTF8;
					resp.ContentType	= new ContentType("text/plain") { CharSet = "UTF-8" }.ToString();
					resp.AddHeader("Access-Control-Allow-Origin","*");
					resp.AddHeader("Cache-Control","no-cache");
				}))
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

		static void Main(string[] args)
		{
			Run<Program>();
		}
	}
}
