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

		static  System.Collections.Concurrent.ConcurrentBag<int> orders = new ConcurrentBag<int>();

		static void Main(string[] args)
        {
            var manager = new Manager();
            var cashier = new Cashier(manager);
            var ass = new AssMan(cashier);

			var cookDispatche = new SmartDispatcher<Order>();
			var cookTTLGate = new TimeToLiveGate<Order>(cookDispatche);
			var cookQueudHandler = new QueuedHandler<Order>(cookTTLGate, "dispatcher");
			var cookLimiter = new Limiter<Order>(cookQueudHandler);
			
			var cookQueudHandler1 = new QueuedHandler<Order>(new Cook(ass, 10000), "c1");
			cookDispatche.AddHandler(cookQueudHandler1);
			var cookQueudHandler2 = new QueuedHandler<Order>(new Cook(ass, 1000), "c2");
			cookDispatche.AddHandler(cookQueudHandler2);
			var cookQueudHandler3 = new QueuedHandler<Order>(new Cook(ass, 100), "c3");	
			cookDispatche.AddHandler(cookQueudHandler3);

			var monitor = new Monitor<Order>(new[] {cookQueudHandler1, cookQueudHandler2, cookQueudHandler3, cookQueudHandler});
		
            //Cook cook = new Cook(ass);
            var waiter = new Waiter(cookLimiter);

			cookQueudHandler1.Start();
			cookQueudHandler2.Start();
			cookQueudHandler3.Start();
			cookQueudHandler.Start();
			monitor.Start();
			new Thread(TryPay).Start(cashier);

            for (int i = 0; i < 100000; i++)
            {
                var orderNumber = waiter.PlaceOrder(new[] { Tuple.Create("Burger", 1) }, 15);
                orders.Add(orderNumber);
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
						cashier.PayForOrder(order);
					}
					catch
					{
					}
				}
			}
		}
    }

    
}
