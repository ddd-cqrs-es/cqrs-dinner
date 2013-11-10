namespace stoplosskata.lessstate
{
	using System.Collections.Generic;
	using System.Linq;

	public class StoplossProcess
	{
		public enum Direction
		{
			Unknown,Even, Up, Down, Completed
		}

		private readonly IBus bus;
		private decimal stopLossPrice;
		private decimal initialDelta;
		private Direction direction;
		private decimal currentPrice;
		private readonly Queue<PriceChanged> minima = new Queue<PriceChanged>();
		private readonly Queue<PriceChanged> maxima = new Queue<PriceChanged>();
		private PriceChanged lastPrice;

		public StoplossProcess(IBus bus)
		{
			this.bus = bus;
		}

		public void Handle(PositionAcquired message)
		{
			stopLossPrice = message.Price * 0.9m;
			initialDelta = message.Price - stopLossPrice;
			currentPrice = message.Price;
			lastPrice = new PriceChanged {Price = message.Price, Symbol = message.Symbol};
			direction= Direction.Unknown;
		}

		public void Handle(PriceChanged message)
		{
			if (direction == Direction.Completed) return;

			//price is moving up
			Direction newstate;
				
			if(message.Price > lastPrice.Price)
			{
				newstate = Direction.Up;
			}
			else if (message.Price < lastPrice.Price) // Price is going down
			{
				newstate = Direction.Down;
			}
			else
			{
				newstate = Direction.Even;
			}

			//detect local minimum
			if ((direction == Direction.Down) && (newstate == Direction.Even || newstate == Direction.Up))
			{
				//privous price is a local minima
				minima.Enqueue(lastPrice);
				//we can already remove any minima that is 
			}
			//enqueue as minimum even values
			if (direction == Direction.Even && (newstate == Direction.Even || newstate == Direction.Up))
			{
				minima.Enqueue(lastPrice);	
			}

			//detect local maximum
			if ((direction == Direction.Up) && (newstate == Direction.Down || newstate== Direction.Even))
			{
				//current price is a mixima
				maxima.Enqueue(lastPrice);
			}
			//enqueue as maximum even values
			if (direction == Direction.Even && (newstate == Direction.Even || newstate == Direction.Down))
			{
				maxima.Enqueue(lastPrice);
			}

			//remember which way the price is going
			direction = newstate;
			lastPrice = message;

			
			
			Publish(new WakeMeUpIn15Seconds { Message = new ShouldWeSell { PriceId = message.Id, Price = message.Price, Symbol = message.Symbol } });	
			
			//trigger price can't go down
			if (message.Price > currentPrice)
			{
				Publish(new WakeMeUpIn20Seconds { Message = new ShouldWeMoveTriggerPrice() {PriceId = message.Id,  Price = message.Price, Symbol = message.Symbol } });
			}

		}

		public void Handle(ShouldWeSell message)
		{
			if (direction == Direction.Completed) return;

			//remove
			if (message.Price < stopLossPrice)
			{
				if (maxima.Count == 0)
				{
					var sell = false;
					if (lastPrice.Id == message.PriceId)
					{
						sell = true;
					}
					else if (lastPrice.Price < message.Price)
					{
						sell = true;
					}
					if (sell)
					{
						bus.Send(new SellPosition { Price = message.Price, Symbol = message.Symbol });
						direction = Direction.Completed;
					}
				}
				else if (maxima.All(x => x.Price < message.Price))
				{
					var sell = false;
					if (lastPrice.Id == message.PriceId)
					{
						sell = true;
					}
					else if (lastPrice.Price < message.Price)
					{
						sell = true;
					}
					if (sell)
					{
						bus.Send(new SellPosition {Price = message.Price, Symbol = message.Symbol});
						direction = Direction.Completed;
					}

				}
			}

			if(minima.Count == 0) return;
			var priceChanged = minima.Peek();
			if (priceChanged.Id == message.PriceId)
			{
				minima.Dequeue();
			}
		}

		public void Handle(ShouldWeMoveTriggerPrice message)
		{
			if (direction == Direction.Completed) return;

			if (message.Price >= currentPrice)
			{
				if (minima.All(x => x.Price >= message.Price) && lastPrice.Price >= message.Price)
				{
					stopLossPrice = message.Price - initialDelta;
					currentPrice = message.Price;
					bus.Publish(new TriggerValueRaised() {TriggerValue = stopLossPrice});

				}
			}

			if(minima.Count==0) return;
			var priceChanged = minima.Peek();
			if (priceChanged.Id == message.PriceId)
			{
				minima.Dequeue();
			}
		}

		private void Publish(IMessage message)
		{
			bus.Publish(message);
		}
	}
}