using System;
using System.IO;
using System.Net;
using System.Threading;

namespace HerokuBase
{
	public abstract class Program
	{
		IAsyncResult asyncResult;
		readonly HttpListener listener	= new HttpListener();
		void Listening(IAsyncResult result)
		{
			var context	= listener.EndGetContext(result);
			asyncResult	= listener.BeginGetContext(Listening,null);

			Listen(context);
		}

		void Stop(object sender,EventArgs e)
		{
			if(asyncResult != null)
				asyncResult.AsyncWaitHandle.Close();

			listener.Stop();
			listener.Close();
		}

		protected abstract bool IsAlive { get; }
		protected abstract void InitListener(HttpListener listener);
		protected abstract void Listen(HttpListenerContext context);
		void Run()
		{
			if(!HttpListener.IsSupported)
				throw new NotSupportedException(typeof(HttpListener).ToString());

			AppDomain.CurrentDomain.DomainUnload	+= Stop;

			InitListener(listener);
			listener.Start();

			asyncResult	= listener.BeginGetContext(Listening,listener);

			while(IsAlive)
				Thread.Sleep(100);
		}

		protected static void Run<P>() where P : Program,new()
		{
			new P().Run();
		}
	}
}
