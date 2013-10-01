namespace Dinner.Messaging
{
	using System.Collections.Generic;
	using System.Linq;

	public class Multiplexer<T> : IHandle<T> where T : IMessage
	{
		private List<IHandle<T>> messageHandlers = new List<IHandle<T>>();

		public void AddHandler(IHandle<T> handler)
		{
			var messageHandlers = this.messageHandlers.ToList();
			messageHandlers.Add(handler);
			this.messageHandlers = messageHandlers;
		}

		public void RemoveHandler(IHandle<T> handler)
		{
			var messageHandlers = this.messageHandlers.ToList();
			messageHandlers.Remove(handler);
			this.messageHandlers = messageHandlers;
		}

		public void Handle(T message)
		{
			foreach (var handler in messageHandlers)
			{
				handler.Handle(message);
			}
		}
	}
}