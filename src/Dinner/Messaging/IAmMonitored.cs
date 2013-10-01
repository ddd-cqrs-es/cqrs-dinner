namespace Dinner.Messaging
{
	public interface IAmMonitored
	{
		string Name { get; }
		int Count();
	}
}