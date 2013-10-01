namespace Dinner.Messaging
{
	using System;

	public interface IHaveTimeToLive
	{
		DateTime TTL { get; }
	}
}