using System;
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
		}

		protected static void Run<P>() where P : Program,new()
		{
			var program	= new P();
			program.Run();
			while(program.IsAlive)
				Thread.Sleep(100);
		}
	}
}
