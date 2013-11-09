namespace stoplosskata.lessstate
{
	using System;
	using System.Collections.Generic;

	public class TriggerValueRaised : Message
		{
			public decimal TriggerValue;
		}

		public class FakeBus : IBus
		{
			public IList<IMessage> PublishedMessages = new List<IMessage>();
			public IList<IMessage> SentMessages = new List<IMessage>();

			public void Publish(IMessage message)
			{
				PublishedMessages.Add(message);
			}

			public void Send(IMessage message)
			{
				SentMessages.Add(message);
			}
		}

		public interface IBus
		{
			void Publish(IMessage message);
			void Send(IMessage sellPosition);
		}

	public abstract class Message: IMessage
		{
			protected Message()
			{
				Id = Guid.NewGuid();
			}
			public Guid Id { get; private set; }

		}

		public class PositionAcquired : Message
		{
			
			public decimal Price;
			public string Symbol;
			
		}

	public class PriceChanged : Message
		{

			public decimal Price;
			public string Symbol;
		}

		public class SellPosition : Message
		{
			public decimal Price;
			public string Symbol;
		}

		public class WakeMeUpIn15Seconds : Message
		{
			public IMessage Message;
		}

		public class ShouldWeSell : Message
		{
			public decimal Price;
			public string Symbol;

			public Guid PriceId;
		
		}

		public class ShouldWeMoveTriggerPrice : Message
		{
			public decimal Price;
			public string Symbol;
			public StoplossProcess.Direction Direction;
			public Guid PriceId;
		}

		

		public interface IMessage
		{
			Guid Id { get; }
		}



		public class WakeMeUpIn20Seconds : Message
		{
			public IMessage Message;
		}
	}
