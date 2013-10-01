namespace Dinner.Messaging
{
	using System.Collections.Concurrent;
	using System.Threading;

	public class QueuedHandler<T> : IAmMonitored, IHandle<T> where T : IMessage
	{
		private readonly ConcurrentQueue<T> queuedMessages = new ConcurrentQueue<T>();
		private readonly IHandle<T> handler;
		private readonly string name;
		private volatile bool canceled;

		public QueuedHandler(IHandle<T> handler, string name)
		{
			this.handler = handler;
			this.name = name;
		}

		public string Name
		{
			get { return name; }
		}

		public int Count()
		{
			return queuedMessages.Count;
		}

		public void Start()
		{
			var t = new Thread(Run);
			t.Start();
		}

		private void Run()
		{
			while (!canceled)
			{
				T message;
				queuedMessages.TryDequeue(out message);
				if (message != null)
				{
					handler.Handle(message);
				}
			}
		}

		public void Stop()
		{
			canceled = true;
		}

		public void Handle(T message)
		{
			queuedMessages.Enqueue(message);
		}
	}
}