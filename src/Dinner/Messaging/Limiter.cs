namespace Dinner.Messaging
{
	using System;
	using System.Threading;

	public class Limiter<T> : IHandle<T> where T : IMessage
	{
		private readonly QueuedHandler<T> handler;

		public Limiter(QueuedHandler<T> handler)
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			if (handler.Count() > 10000)
			{
				Console.Write(".");
				Thread.Sleep(1);
			}
			handler.Handle(message);
		}
	}
}