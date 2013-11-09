namespace Dinner.Messaging
{
	using System;

	public interface IMessage
	{
		Guid Id { get; }
		Guid CausationId { get; }
		Guid CorrelationId { get; }
	}
}