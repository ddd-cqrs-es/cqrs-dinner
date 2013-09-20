namespace Dinner
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading;

	static class Program
    {

		static readonly ConcurrentDictionary<Guid, object> orders = new ConcurrentDictionary<Guid, object>();

		static void Main(string[] args)
		{
			var d = new Dispatcher();
			var midgetHouse = new MidgetHouse(d);
			d.Subscribe<OrderPlaced>(midgetHouse);
			d.Subscribe<DodgyOrderPlaced>(midgetHouse);
			var manager = new Manager();
            var cashier = new Cashier(d);
            var ass = new AssMan(d);

			var cookDispatcher = new SmartDispatcher<CookFood>();
			var cookTtlGate = new TimeToLiveGate<CookFood>(cookDispatcher);
			var cookQueudHandler = new QueuedHandler<CookFood>(cookTtlGate, "dispatcher");
			var cookLimiter = new Limiter<CookFood>(cookQueudHandler);
			var cookScrewsUp = new ScrewsUp<CookFood>(cookLimiter);

			var alarmClock = new AlarmClock(d);

			var messageMonitor = new MessageMonitor(d);

			d.Subscribe(alarmClock);
			d.Subscribe(cookScrewsUp);
			d.Subscribe(ass);
			d.Subscribe(cashier);
			d.Subscribe(manager);
			d.Subscribe<OrderPlaced>(messageMonitor);
			d.Subscribe<DodgyOrderPlaced>(messageMonitor);

			var cookQueudHandler1 = new QueuedHandler<CookFood>(new Cook(d, 10000), "c1");
			cookDispatcher.AddHandler(cookQueudHandler1);
			var cookQueudHandler2 = new QueuedHandler<CookFood>(new Cook(d, 1000), "c2");
			cookDispatcher.AddHandler(cookQueudHandler2);
			var cookQueudHandler3 = new QueuedHandler<CookFood>(new Cook(d, 100), "c3");	
			cookDispatcher.AddHandler(cookQueudHandler3);


			var queueMonitor = new QueueMonitor(new[] {cookQueudHandler1, cookQueudHandler2, cookQueudHandler3, cookQueudHandler});
			


            //Cook cook = new Cook(ass);
            var waiter = new Waiter(d);

			cookQueudHandler1.Start();
			cookQueudHandler2.Start();
			cookQueudHandler3.Start();
			cookQueudHandler.Start();
			alarmClock.Start();
			queueMonitor.Start();
			
			new Thread(TryPay).Start(cashier);

			Random r = new Random();
            for (int i = 0; i < 10000; i++)
            {
	            Guid orderNumber;
	            if (r.Next()%2 == 0)
				{
					orderNumber = waiter.PlaceOrder(new[] {Tuple.Create("Burger", 1)}, 15);

				}
				else
				{
					orderNumber = waiter.PlaceDodgyOrder(new[] { Tuple.Create("Burger", 1) }, 15);
				}

	            orders.TryAdd(orderNumber, null);
		    }
            //var orderNumber = waiter.PlaceOrder(new[] {Tuple.Create("Burger", 1)}, 15);
            //cashier.PayForOrder(orderNumber);
            Console.ReadLine();
        }

		private static void TryPay(object c)

		{
			var cashier = (Cashier) c;
			while (true)
			{
				foreach (var order in orders)
				{
					try
					{
						cashier.PayForOrder(order.Key);
						object val;
						orders.TryRemove(order.Key,out val);
					}
// ReSharper disable EmptyGeneralCatchClause
					catch
// ReSharper restore EmptyGeneralCatchClause
					{
					}
				}
			}
		}
    }

    
}
