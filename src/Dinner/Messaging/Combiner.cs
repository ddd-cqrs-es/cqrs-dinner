namespace Dinner.Messaging
{
	public class Combiner<T> : IHandle<T> where T : IMessage
	{
		private IHandle<T> handler;

		public Combiner(IHandle<T> handler)
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			handler.Handle(message);
		}
	}
}