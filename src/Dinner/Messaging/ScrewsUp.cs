namespace Dinner.Messaging
{
	using System;

	public class ScrewsUp<T>: IHandle<T> where T: IMessage
	{
		private readonly IHandle<T> next;
		private readonly Random rnd;

		public ScrewsUp(IHandle<T> next)
		{
			this.next = next;
			rnd = new Random();
		}

		public void Handle(T message)
		{
			var nextDouble = rnd.NextDouble();
			if(nextDouble < 0.95) return;
			next.Handle(message);
		}
	}
}