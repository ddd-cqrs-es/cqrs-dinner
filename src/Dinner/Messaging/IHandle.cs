namespace Dinner.Messaging
{
	public interface IHandle<T>
	{
		void Handle(T message);
	}
}