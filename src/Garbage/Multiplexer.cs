namespace Garbage
{
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;

	public class QueuedHandler<T> :IHandle<T> where T: IMessage
	{
		private readonly IHandle<T> handle;
		private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
		private volatile bool running = true;

		public QueuedHandler(IHandle<T> handle)
		{
			this.handle = handle;
		}

		public void Start()
		{
			var t = new Thread(DequeuMessages);
			t.Start();

		}

		public void Stop()
		{
			running = false;
		}

		private void DequeuMessages()
		{
			while (running)
			{
				T message= default(T);
				queue.TryDequeue(out message);
				if (message != null)
				{
					handle.Handle(message);
				}

				Thread.Sleep(1);
			}
		}

		public void Handle(T message)
		{
			queue.Enqueue(message);
		}
	}

	public class RoundRobinDispatcher<T>: IHandle<T> where T : IMessage
	{
		private readonly ConcurrentQueue<IHandle<T>> queue = new ConcurrentQueue<IHandle<T>>();

		public RoundRobinDispatcher(IEnumerable<IHandle<T>> handlers)
		{
			foreach (var handler in handlers)
			{
				queue.Enqueue(handler);
			}
		}

		public void Handle(T message)
		{
			IHandle<T> handler;
			queue.TryDequeue(out handler);
			handler.Handle(message);
			queue.Enqueue(handler);
		}
	}

	public class Combiner<T>:IHandle<T> where T : IMessage
	{
		private readonly IHandle<T> handle;

		public Combiner(IHandle<T>	handle)
		{
			this.handle = handle;
		}

		public void Handle(T message)
		{
			handle.Handle(message);
		}
	}

	public class Multiplexer<T>: IHandle<T> where T : IMessage
	{
		private List<IHandle<T>> handlers;

		public Multiplexer(IEnumerable<IHandle<T>> handlers)
		{
			this.handlers = handlers.ToList();
		} 
		public void Handle(T message)
		{
			foreach (var handler in handlers)
			{
				handler.Handle(message);
			}
		}

		public void Add(IHandle<T> handler)
		{
			var h = handlers.Select(x => x).ToList();
			h.Add(handler);

			handlers = h;
		}

		public void Remove(IHandle<T> handler)
		{
			var h = handlers.Where(x => !x.Equals(handler)).ToList();
			handlers = h;
		}
	}

	public interface IHandle<T> where T: IMessage
	{
		void Handle(T message);
	}

	public interface IMessage{}
}
