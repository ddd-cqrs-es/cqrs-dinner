namespace Dinner
{
	using System;
	using System.Collections.Generic;

	public class MidgetHouse: IHandle<OrderPlaced>, IHandle<OrderCompleted>
	{
		private readonly Dictionary<Guid,IMidget> midgets = new Dictionary<Guid, IMidget>();
		private readonly Dispatcher dispatcher;

		public MidgetHouse(Dispatcher dispatcher)
		{
			this.dispatcher = dispatcher;
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

	internal interface IMidget
	{

	}

	public class MidgetFactory
	{
		
	}

	public class PayAfterMidget:IMidget, IHandle<FoodPrepared>, IHandle<OrderPriced>, IHandle<OrderPaid>, IHandle<OrderPlaced>
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