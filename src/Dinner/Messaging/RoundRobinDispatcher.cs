namespace Dinner.Messaging
{
	using System.Collections.Generic;

	public class RoundRobinDispatcher<T> : IHandle<T> where T : IMessage
	{
		private readonly Queue<IHandle<T>> messageHandlers = new Queue<IHandle<T>>();

		public void Handle(T message)
		{
			var handler = messageHandlers.Dequeue();
			messageHandlers.Enqueue(handler);

			handler.Handle(message);
		}

		public void AddHandler(IHandle<T> handler)
		{
			messageHandlers.Enqueue(handler);
		}
	}
}