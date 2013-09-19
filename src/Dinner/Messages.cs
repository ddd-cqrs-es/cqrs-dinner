namespace Dinner
{
	using System;

	public class FoodPrepared : IMessage
	{
		public FoodPrepared()
		{
			Id = Guid.NewGuid();
		}

		public Guid CausationId { get; set; }
		public Guid CorolationId { get; set; }
		public Order Order;
		public Guid Id { get; private set; }

		public override string ToString()
		{
			return "Food prepared";
		}
	}

	public class OrderPlaced : IMessage, IHaveTimeToLive
	{
		public OrderPlaced()
		{
			Id = Guid.NewGuid();
			CorolationId = Id;
		}

		public override string ToString()
		{
			return "Order placed";
		}

		public Guid CausationId { get; set; }
		public Guid CorolationId { get; set; }
		public Order Order;
		public Guid Id { get; set; }

		public DateTime TTL
		{
			get { return Order.TTL; }
		}
	}

	public class OrderPriced : IMessage
	{
		public OrderPriced()
		{
			Id = Guid.NewGuid();
		}

		public Guid CausationId { get; set; }
		public Guid CorolationId { get; set; }
		public Guid Id { get; private set; }
		public Order Order;

		public override string ToString()
		{
			return "Order priced";
		}
	}

	public class OrderPaid : IMessage
	{
		public OrderPaid()
		{
			Id = Guid.NewGuid();
		}

		public Guid CausationId { get; set; }
		public Guid CorolationId { get; set; }
		public Order Order;
		public Guid Id { get; private set; }

		public override string ToString()
		{
			return "Order Paid";
		}
	}
}