namespace Dinner.Messaging
{
	using System;

	public class TimeToLiveGate<T> : IHandle<T> where T : IHaveTimeToLive, IMessage
	{
		private readonly IHandle<T> handler;

		public TimeToLiveGate(IHandle<T> handler)
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			if (!IsExpired(message))
			{
				handler.Handle(message);
			}
			else
			{
				Console.WriteLine("Dropped message {0}", message.Id);
			}
		}

		private bool IsExpired(T message)
		{
			return message.TTL < DateTime.Now;
		}
	}
}