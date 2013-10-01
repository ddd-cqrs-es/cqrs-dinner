namespace Dinner.Messaging
{
	using System.Collections.Generic;

	public class SmartDispatcher<T> : IHandle<T> where T : IMessage
	{
		private readonly Queue<QueuedHandler<T>> messageHandlers = new Queue<QueuedHandler<T>>();

		public void Handle(T message)
		{
			var handled = false;
			while (handled == false)
			{
				var handler = messageHandlers.Dequeue();
				if (handler.Count() < 3)
				{
					handler.Handle(message);
					handled = true;
				}
				messageHandlers.Enqueue(handler);
			}
		}

		public void AddHandler(QueuedHandler<T> handler)
		{
			messageHandlers.Enqueue(handler);
		}
	}
}