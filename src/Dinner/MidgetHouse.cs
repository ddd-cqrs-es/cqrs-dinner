namespace Dinner
{
	using System;
	using System.Collections.Generic;

	public class MidgetHouse: IHandle<OrderPlaced>, IHandle<OrderCompleted>, IHandle<DodgyOrderPlaced>
	{
		private readonly Dictionary<Guid,IMidget> midgets = new Dictionary<Guid, IMidget>();
		private readonly Dispatcher dispatcher;

		public MidgetHouse(Dispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}

		public void Handle(DodgyOrderPlaced m)
		{

			var midget = new PayBeforeMidget(dispatcher);
			dispatcher.Subscribe<FoodPrepared>(midget, m.CorolationId.ToString());
			dispatcher.Subscribe<OrderPriced>(midget, m.CorolationId.ToString());
			dispatcher.Subscribe<OrderPaid>(midget, m.CorolationId.ToString());

			midgets.Add(m.CorolationId, midget);

			midget.Handle(m);
			
	
		}

		public void Handle(OrderPlaced message)
		{

			
			var midget = new PayAfterMidget(dispatcher);
			dispatcher.Subscribe<FoodPrepared>(midget, message.CorolationId.ToString());
			dispatcher.Subscribe<OrderPriced>(midget, message.CorolationId.ToString());
			dispatcher.Subscribe<OrderPaid>(midget, message.CorolationId.ToString());
			
			midgets.Add(message.CorolationId, midget);

			midget.Handle(message);
			
	

		}

		public void Handle(OrderCompleted message)
		{
			
			midgets.Remove(message.CorolationId);
		}
	}

	public interface IMidget<TTrigger>: IMidget
	{

	}

	public interface IMidget{}


	public class PayBeforeMidget : IHandle<FoodPrepared>, IHandle<OrderPriced>, IHandle<OrderPaid>, IHandle<DodgyOrderPlaced>, IMidget<DodgyOrderPlaced>
	{
		private readonly Dispatcher dispatcher;

		public PayBeforeMidget(Dispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}

		public void Handle(FoodPrepared message)
		{
			dispatcher.Publish(new OrderCompleted() { CausationId = message.Id, CorolationId = message.CorolationId, Order = message.Order });
		}

		public void Handle(OrderPriced message)
		{
			dispatcher.Publish(new PrepareForPayment() { CausationId = message.Id, CorolationId = message.CorolationId, Order = message.Order });
		}

		public void Handle(OrderPaid message)
		{
			dispatcher.Publish(new CookFood() { CausationId = message.Id, CorolationId = message.CorolationId, Order = message.Order, TTL = message.Order.TTL});
		}

		public void Handle(DodgyOrderPlaced message)
		{
			dispatcher.Publish(new PriceOrder() { CausationId = message.Id, CorolationId = message.CorolationId, Order = message.Order });
		}
	}

	public class PayAfterMidget:IMidget<OrderPlaced>, IHandle<FoodPrepared>, IHandle<OrderPriced>, IHandle<OrderPaid>, IHandle<OrderPlaced>
	{
		private readonly Dispatcher dispatcher;

		public PayAfterMidget(Dispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
		}


		public void Handle(FoodPrepared message)
		{
			dispatcher.Publish(new PriceOrder{CausationId= message.Id, CorolationId = message.CorolationId, Order = message.Order});	
		}

		public void Handle(OrderPriced message)
		{
			dispatcher.Publish(new PrepareForPayment() { CausationId = message.Id, CorolationId = message.CorolationId, Order = message.Order });		
		}

		public void Handle(OrderPaid message)
		{
			dispatcher.Publish(new OrderCompleted() { CausationId = message.Id, CorolationId = message.CorolationId, Order = message.Order });		
		}

		public void Handle(OrderPlaced message)
		{
			dispatcher.Publish(new CookFood() { CausationId = message.Id, CorolationId = message.CorolationId, Order = message.Order, TTL = message.TTL});		
		}
	}
}