namespace stoplosskata
{
    using System;
    using System.Collections.Generic;

	public class Stoploss: IConsume<PositionAcquiredMessage>, IConsume<PriceChangedMessage>
    {
	    private Guid positionId;
	    private double stopAt;
	    private readonly IList<Message> producedMessages= new List<Message>();

	    public void Consume(PositionAcquiredMessage message)
        {
	        positionId = message.PositionId;
	        stopAt = message.Price - message.StopLoss;
        }
        public void Consume(PriceChangedMessage message)
        {
	        if (message.Price <= stopAt)
	        {
				producedMessages.Add(new SellPositionMessage{PositionId = positionId});    
	        }
        }
        

        public IEnumerable<Message> ProducedMessages
        {
	        get {
		        foreach (var message in producedMessages)
		        {
			        yield return message;
		        }
		    }
	       
        }
    }

    public class SellPositionMessage: Message, IEquatable<SellPositionMessage>
    {
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
		    if (obj.GetType() != this.GetType())
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

	    public Guid PositionId { get; set; }
    }

    public class Message
    {
	    public Message()
	    {
			Id = Guid.NewGuid();
	    }

	    public Guid Id ;
    }

    public class PositionAcquiredMessage: Message
    {
	    public Guid PositionId { get; set; }
	    public double Price { get; set; }

	    public double StopLoss { get; set; }
    }

    public class PriceChangedMessage: Message
    {
	    public double Price { get; set; }
    }

	public interface IConsume<T> where T: Message
	{
		void Consume(T message);
	}
}