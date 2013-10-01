namespace Dinner.Messaging
{
	public class Widener<T, U> : IHandle<T> where T : U
	{
		private readonly IHandle<U> handler;

		public Widener(IHandle<U> handler)
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			handler.Handle(message);
		}
	}
}