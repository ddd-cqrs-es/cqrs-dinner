namespace Dinner
{
	using System;
	using System.Collections.Generic;
	using Messaging;

	public class MidgetHouse: IHandle<OrderPlaced>, IHandle<DodgyOrderPlaced>, IHandle<OrderCompleted>
	{
		private readonly Dictionary<Guid,IMidget> midgets = new Dictionary<Guid, IMidget>();
		private readonly Dispatcher dispatcher;

		public MidgetHouse(Dispatcher dispatcher )
		{
			this.dispatcher = dispatcher;	
		}

		public void Handle(DodgyOrderPlaced m)
		{

			var midget = new PayBeforeMidget(dispatcher);
			dispatcher.Subscribe<FoodPrepared>(midget, m.CorrelationId.ToString());
			dispatcher.Subscribe<OrderPriced>(midget, m.CorrelationId.ToString());
			dispatcher.Subscribe<OrderPaid>(midget, m.CorrelationId.ToString());
			dispatcher.Subscribe<CookingTimedOut>(midget, m.CorrelationId.ToString());
			midgets.Add(m.CorrelationId, midget);

			midget.Handle(m);
			
	
		}

		public void Handle(OrderPlaced message)
		{
			var midget = new PayAfterMidget(dispatcher);
			dispatcher.Subscribe<FoodPrepared>(midget, message.CorrelationId.ToString());
			dispatcher.Subscribe<OrderPriced>(midget, message.CorrelationId.ToString());
			dispatcher.Subscribe<OrderPaid>(midget, message.CorrelationId.ToString());
			dispatcher.Subscribe<CookingTimedOut>(midget, message.CorrelationId.ToString());
			midgets.Add(message.CorrelationId, midget);

			midget.Handle(message);
			
		}

		public void Handle(OrderCompleted message)
		{
			midgets.Remove(message.CorrelationId);
		}
	}

	public interface IMidget<TTrigger>: IMidget
	{

	}

	public interface IMidget{}


	public class PayBeforeMidget : IHandle<FoodPrepared>, IHandle<OrderPriced>, IHandle<OrderPaid>, IHandle<DodgyOrderPlaced>, IMidget<DodgyOrderPlaced>, IHandle<CookingTimedOut>
	{
		private readonly Dispatcher dispatcher;
		private bool foodPrepared;

		public PayBeforeMidget(Dispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}

		public void Handle(FoodPrepared message)
		{
			foodPrepared = true;
			dispatcher.Handle(new OrderCompleted() { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order });
		}

		public void Handle(OrderPriced message)
		{
			dispatcher.Handle(new PrepareForPayment() { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order });
		}

		public void Handle(OrderPaid message)
		{
			dispatcher.Handle(new WakeMeIn(){ CausationId= message.Id, CorrelationId = message.CorrelationId, TTL = 3, Message =  new CookingTimedOut(){CausationId = message.Id, CorrelationId= message.CorrelationId, Order = message.Order}});
			dispatcher.Handle(new CookFood() { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order, TTL = message.Order.TTL});
		}

		public void Handle(DodgyOrderPlaced message)
		{
			dispatcher.Handle(new PriceOrder() { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order });
		}

		public void Handle(CookingTimedOut message)
		{
			if(!foodPrepared)
				dispatcher.Handle(new CookFood{CausationId = message.Id, CorrelationId =  message.CorrelationId, Order = message.Order});
		}
	}

	public class CookingTimedOut : IMessage
	{
		public CookingTimedOut()
		{
			Id = Guid.NewGuid();
		}

		public Order Order { get; set; }

		public Guid Id { get; private set; }
		public Guid CausationId { get; set; }
		public Guid CorrelationId { get; set; }
	}

	public class PayAfterMidget:IMidget<OrderPlaced>, IHandle<FoodPrepared>, IHandle<OrderPriced>, IHandle<OrderPaid>, IHandle<OrderPlaced>, IHandle<CookingTimedOut>
	{
		private readonly Dispatcher dispatcher;
		private bool foodPrepared;

		public PayAfterMidget(Dispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}


		public void Handle(FoodPrepared message)
		{
			foodPrepared = true;
			dispatcher.Handle(new PriceOrder{CausationId= message.Id, CorrelationId = message.CorrelationId, Order = message.Order});	
		}

		public void Handle(OrderPriced message)
		{
			dispatcher.Handle(new PrepareForPayment() { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order });		
		}

		public void Handle(OrderPaid message)
		{
			dispatcher.Handle(new OrderCompleted() { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order });		
		}

		public void Handle(OrderPlaced message)
		{
			dispatcher.Handle(new WakeMeIn() { CausationId = message.Id, CorrelationId = message.CorrelationId, TTL = 3, Message = new CookingTimedOut() { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order } });
			dispatcher.Handle(new CookFood() { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order, TTL = message.TTL});		
		}

		public void Handle(CookingTimedOut message)
		{
			if (!foodPrepared)
				dispatcher.Handle(new CookFood { CausationId = message.Id, CorrelationId = message.CorrelationId, Order = message.Order });
		}
	}
}