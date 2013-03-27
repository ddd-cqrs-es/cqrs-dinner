namespace stoplosskata
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class Stoploss : IConsume<PositionAcquiredMessage>, IConsume<PriceChangedMessage>, IConsume<Elapsed15Sec>,
	                        IConsume<Elapsed35Sec>
	{
		private readonly IList<Message> producedMessages = new List<Message>();
		private Guid positionId;
		private double stopAt;
		private double stopLoss;
		private IList<PriceChangedMessage> PriceChanges=new List<PriceChangedMessage>();

		public IEnumerable<Message> ProducedMessages
		{
			get
			{
				foreach (Message message in producedMessages)
				{
					yield return message;
				}
			}
		}

		public void Consume(Elapsed15Sec message)
		{
			var priceChangedMessage = PriceChanges.FirstOrDefault(x => x.Id == message.ChangeId);
			if (priceChangedMessage != null)
			{
				var at = priceChangedMessage.Price - stopLoss;
				if (at > stopAt)
				{
					stopAt = at;	
				}
			}
		}

		public void Consume(Elapsed35Sec message)
		{
//			if (message.Price <= stopAt)
//			{
//				producedMessages.Add(new SellPositionMessage { PositionId = positionId });
//			}
		}

		public void Consume(PositionAcquiredMessage message)
		{
			positionId = message.PositionId;
			stopLoss = message.StopLoss;
			stopAt = message.Price - stopLoss;
		}

		public void Consume(PriceChangedMessage message)
		{
			this.PriceChanges.Add(message);
			this.Publish(new WaitMessage<Elapsed15Sec>(new Elapsed15Sec {ChangeId = message.Id}, 15));
		}

		private void Publish<T>(WaitMessage<T> waitMessage)
		{
			this.producedMessages.Add(waitMessage);
		}
	}

	public class WaitMessage<TMessage>: Message
	{
		private readonly TMessage message;
		private readonly int wait;

		public WaitMessage(TMessage message, int wait)
		{
			this.message = message;
			this.wait = wait;
			throw new NotImplementedException();
		}
	}

	public class Elapsed35Sec : Message
	{
	}

	public class Elapsed15Sec : Message
	{
		public Guid ChangeId { get; set; }
	}

	public class SellPositionMessage : Message, IEquatable<SellPositionMessage>
	{
		public Guid PositionId { get; set; }

		public bool Equals(SellPositionMessage other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return PositionId.Equals(other.PositionId);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((SellPositionMessage) obj);
		}

		public override int GetHashCode()
		{
			return PositionId.GetHashCode();
		}

		public static bool operator ==(SellPositionMessage left, SellPositionMessage right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SellPositionMessage left, SellPositionMessage right)
		{
			return !Equals(left, right);
		}
	}

	public class Message
	{
		public Guid Id;

		public Message()
		{
			Id = Guid.NewGuid();
		}
	}

	public class PositionAcquiredMessage : Message
	{
		public Guid PositionId { get; set; }
		public double Price { get; set; }

		public double StopLoss { get; set; }
	}

	public class PriceChangedMessage : Message
	{
		public double Price { get; set; }
	}

	public interface IConsume<T> where T : Message
	{
		void Consume(T message);
	}
}