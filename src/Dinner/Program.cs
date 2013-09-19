using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentProblem;

namespace ConsoleApplication1
{
	using System.Collections.Concurrent;
	using System.Threading;

	class Program
    {

		static  ConcurrentDictionary<int, object> orders = new ConcurrentDictionary<int, object>();

		static void Main(string[] args)
		{
			var d = new Dispatcher();
            var manager = new Manager();
            var cashier = new Cashier(d);
            var ass = new AssMan(d);

			var cookDispatcher = new SmartDispatcher<Order>();
			var cookTTLGate = new TimeToLiveGate<Order>(cookDispatcher);
			var cookQueudHandler = new QueuedHandler<Order>(cookTTLGate, "dispatcher");
			var cookLimiter = new Limiter<Order>(cookQueudHandler);
			
			d.Subscribe(cookLimiter, "order-created");
			d.Subscribe(ass, "order-ready");
			d.Subscribe(cashier, "order-priced");
			d.Subscribe(manager, "order-paid");

			var cookQueudHandler1 = new QueuedHandler<Order>(new Cook(d, 10000), "c1");
			cookDispatcher.AddHandler(cookQueudHandler1);
			var cookQueudHandler2 = new QueuedHandler<Order>(new Cook(d, 1000), "c2");
			cookDispatcher.AddHandler(cookQueudHandler2);
			var cookQueudHandler3 = new QueuedHandler<Order>(new Cook(d, 100), "c3");	
			cookDispatcher.AddHandler(cookQueudHandler3);

			var monitor = new Monitor<Order>(new[] {cookQueudHandler1, cookQueudHandler2, cookQueudHandler3, cookQueudHandler});
		
            //Cook cook = new Cook(ass);
            var waiter = new Waiter(d);

			cookQueudHandler1.Start();
			cookQueudHandler2.Start();
			cookQueudHandler3.Start();
			cookQueudHandler.Start();
			monitor.Start();
			new Thread(TryPay).Start(cashier);

            for (int i = 0; i < 100000; i++)
            {
                var orderNumber = waiter.PlaceOrder(new[] { Tuple.Create("Burger", 1) }, 15);
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
					catch
					{
					}
				}
			}
		}
    }

    
}
