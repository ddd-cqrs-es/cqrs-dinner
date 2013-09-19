using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentProblem
{
	public class Multiplexer<T> : IHandle<T> where T : IMessage
	{
		private List<IHandle<T>> messageHandlers = new List<IHandle<T>>();

		public void AddHandler(IHandle<T> handler)
		{
			var messageHandlers = this.messageHandlers.ToList();
			messageHandlers.Add(handler);
			this.messageHandlers = messageHandlers;
		}

		public void RemoveHandler(IHandle<T> handler)
		{
			var messageHandlers = this.messageHandlers.ToList();
			messageHandlers.Remove(handler);
			this.messageHandlers = messageHandlers;
		}

		public void Handle(T message)
		{
			foreach (var handler in messageHandlers) handler.Handle(message);
		}
	}

	public class Combiner<T> : IHandle<T> where T : IMessage
	{
		private IHandle<T> handler;

		public Combiner(IHandle<T> handler)
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			handler.Handle(message);
		}
	}


	public class Narrower<T, U> : IHandle<T>, IEquatable<Narrower<T, U>> where U: T
	{
		public bool Equals(Narrower<T, U> other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return handler.Equals(other.handler);
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
			return Equals((Narrower<T, U>) obj);
		}

		public override int GetHashCode()
		{
			return handler.GetHashCode();
		}

		private readonly IHandle<U> handler;

		public Narrower(IHandle<U> handler )
		{
			this.handler = handler;
		} 
		public void Handle(T message)
		{
			handler.Handle((U) message);
		}
	}

	public class Widener<T,U>: IHandle<T> where T:U
	{
		private readonly IHandle<U> handler;

		public Widener(IHandle<U> handler )
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			handler.Handle(message);
		}
	}

	public class Dipatcher
	{
		private readonly Dictionary<string, Multiplexer<IMessage>> topics = new Dictionary<string, Multiplexer<IMessage>>(); 
		 

		public void Publish<T>(string topic, T message) where T : IMessage
		{
			new Widener<T, IMessage>(topics[topic]).Handle(message);
		}

		public void Subscribe<T>(IHandle<T> handler, string topic ) where T : IMessage
		{
			if(!topics.ContainsKey(topic))
			{
				topics.Add(topic, new Multiplexer<IMessage>());
			}

			var multi = topics[topic];

			multi.AddHandler(new Narrower<IMessage, T>(handler));
		}

		public void Unsubscribe<T>(IHandle<T> handler, string topic) where T : IMessage
		{
			var multi = topics[topic];

			multi.RemoveHandler(new Narrower<IMessage, T>(handler));
		}




	}

	
	
	public class Monitor<T> where T : IMessage
	{
		private IEnumerable<QueuedHandler<T>> handlers;

		public Monitor(IEnumerable<QueuedHandler<T>> handlers)
		{
			this.handlers = handlers;
		}

		public void Start()
		{
			new Thread(PrintToConsole).Start();
		}

		private void PrintToConsole()
		{
			while (true)
			{

				foreach (var queuedHandler in handlers)
				{
					Console.WriteLine("Queue {0} count {1}", queuedHandler.name, queuedHandler.Count());
				}
				Thread.Sleep(1000);
			}
		}
	}

	public class QueuedHandler<T> : IHandle<T> where T : IMessage
	{
		private ConcurrentQueue<T> queuedMessages = new ConcurrentQueue<T>();
		private IHandle<T> handler;
		public readonly string name;
		private volatile bool canceled;

		public QueuedHandler(IHandle<T> handler, string name)
		{
			this.handler = handler;
			this.name = name;
		}

		private void PrintCount()
		{
			Console.WriteLine("Queue {0} count {1}", name, queuedMessages.Count);
		}

		public int Count()
		{
			return queuedMessages.Count;
		}



		public void Start()
		{
			CancellationToken cancellationToken = new CancellationToken();
			var task = Task.Factory.StartNew(() =>
				{
					while (!canceled)
					{
						T message;
						if (queuedMessages.Any() && queuedMessages.TryDequeue(out message)) handler.Handle(message);
					}
				}, cancellationToken);
		}

		private void PrintEverySecond()
		{
			while (true)
			{
				PrintCount();

			}
		}

		public void Stop()
		{
			canceled = true;
		}

		public void Handle(T message)
		{
			queuedMessages.Enqueue(message);
		}
	}

	public class TimeToLiveGate<T> : IHandle<T> where T : IHaveTimeToLive, IMessage
	{
		private readonly IHandle<T> handler;

		public TimeToLiveGate(IHandle<T> handler)
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			if (!IsExpired(message))
			{
				handler.Handle(message);
			}
			else
			{
				Console.WriteLine("Dropped message {0}", message.Id);
			}
		}

		private bool IsExpired(T message)
		{
			return message.TTL > DateTime.Now;
		}
	}

	public interface IHaveTimeToLive
	{
		DateTime TTL { get; }
	}

	public class SmartDispatcher<T> : IHandle<T> where T : IMessage
	{
		private readonly Queue<QueuedHandler<T>> messageHandlers = new Queue<QueuedHandler<T>>();

		public void Handle(T message)
		{
			var handled = false;
			while (handled == false)
			{
				var handler = messageHandlers.Dequeue();
				if (handler.Count() < 3)
				{
					handler.Handle(message);
					handled = true;
				}
				messageHandlers.Enqueue(handler);
			}



		}

		public void AddHandler(QueuedHandler<T> handler)
		{
			messageHandlers.Enqueue(handler);
		}
	}

	public class Limiter<T>: IHandle<T> where T: IMessage
	{
		private readonly QueuedHandler<T> handler;

		public Limiter(QueuedHandler<T> handler )
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			if (handler.Count() > 10000)
			{
				Console.WriteLine("Limiting");
				Thread.Sleep(1);
			}
			handler.Handle(message);
		}
	}

	public class RoundRobinDispatcher<T> : IHandle<T> where T : IMessage
	{
		private readonly Queue<IHandle<T>> messageHandlers = new Queue<IHandle<T>>();

		public void Handle(T message)
		{
			var handler = messageHandlers.Dequeue();
			messageHandlers.Enqueue(handler);

			handler.Handle(message);
		}

		public void AddHandler(IHandle<T> handler)
		{
			messageHandlers.Enqueue(handler);
		}
	}

	public interface IMessage
	{
		int Id { get; }
	}

	public interface IHandle<T>
	{
		void Handle(T message);
	}

	public class Waiter
	{
		private int lastOrderNumber;
		private IHandle<Order> handlesOrder;

		public Waiter(IHandle<Order> handlesOrder)
		{

			this.handlesOrder = handlesOrder;
		}

		public int PlaceOrder(IEnumerable<Tuple<string, int>> items, int tableNumber)
		{
			Order order = new Order();
			order.OrderNumber = ++lastOrderNumber;
			order.TableNumber = tableNumber;
			order.Created = DateTime.Now;
			order.TTL = order.Created.AddSeconds(3);
			foreach (var item in items) order.AddItem(item.Item1, item.Item2);
			handlesOrder.Handle(order);
			return order.OrderNumber;
		}
	}

	public class Cook : IHandle<Order>
	{
		private Dictionary<string, List<string>> ingredientsByDishName = new Dictionary<string, List<string>>();
		private IHandle<Order> handlesOrder;
		private readonly int speed;

		public Cook(IHandle<Order> handlesOrder, int speed)
		{
			ingredientsByDishName.Add("Burger", new List<string> {"Bun", "Meat"});

			this.handlesOrder = handlesOrder;
			this.speed = speed;
		}

		public void Handle(Order message)
		{
			foreach (var item in message.Items)
			{
				if (ingredientsByDishName.ContainsKey(item.Name)) item.Ingredients = ingredientsByDishName[item.Name].ToList();
				else throw new Exception("I have no idea how to cook this!!!!");

				Console.WriteLine("Cooking: " + item.Name);
			}

			Thread.Sleep(speed);
			handlesOrder.Handle(message);
		}
	}

	public class AssMan : IHandle<Order>
	{
		private decimal taxRate = 0.07m;
		private Dictionary<string, decimal> pricesByDishName = new Dictionary<string, decimal>();
		private IHandle<Order> handlesOrder;

		public AssMan(IHandle<Order> handlesOrder)
		{
			this.handlesOrder = handlesOrder;
			pricesByDishName.Add("Burger", 10);
		}

		public void Handle(Order message)
		{
			foreach (var item in message.Items)
			{
				if (pricesByDishName.ContainsKey(item.Name)) item.Price = pricesByDishName[item.Name]*item.Qty;
				else throw new Exception("I have no idea what the price is!!!!");

				Console.WriteLine("Calculating price..");
			}

			message.SubTotal = message.Items.Sum(i => i.Price);
			message.Total = message.SubTotal + (message.SubTotal*taxRate);

			handlesOrder.Handle(message);
		}
	}

	public class Cashier : IHandle<Order>
	{
		private Dictionary<int, Order> ordersAwaitingPaymentByOrderNumber = new Dictionary<int, Order>();
		private IHandle<Order> handlesOrder;

		public Cashier(IHandle<Order> handlesOrder)
		{
			this.handlesOrder = handlesOrder;
		}

		public void Handle(Order message)
		{
			ordersAwaitingPaymentByOrderNumber.Add(message.OrderNumber, message);
		}

		public void PayForOrder(int orderNumber)
		{
			var order = ordersAwaitingPaymentByOrderNumber[orderNumber];
			order.IsPaid = true;

			Console.WriteLine("Paying for order " + orderNumber);
			handlesOrder.Handle(order);
		}
	}

	public class Manager : IHandle<Order>
	{
		public void Handle(Order message)
		{
			message.Completed = DateTime.Now;

			Console.WriteLine(message.OrderNumber + " is completed!");
			Console.WriteLine("Processing time for order {0} was {1}", message.OrderNumber,
			                  message.Completed.Subtract(message.Created));
		}
	}

	public class Order : IMessage, IHaveTimeToLive
	{
		private JObject json;

		public Order() : this(new JObject())
		{

		}

		public Order(JObject json)
		{
			this.json = json;
		}

		public int OrderNumber
		{
			get { return (int) json["OrderNumber"]; }
			set { json["OrderNumber"] = value; }
		}

		public int TableNumber
		{
			get { return (int) json["v"]; }
			set { json["TableNumber"] = value; }
		}

		public IEnumerable<Item> Items
		{
			get { return json["Items"].Select(i => new Item(i)); }
		}

		public decimal SubTotal
		{
			get { return (decimal) json["SubTotal"]; }
			set { json["SubTotal"] = value; }
		}

		public decimal Total
		{
			get { return (decimal) json["Total"]; }
			set { json["Total"] = value; }
		}

		public bool IsPaid
		{
			get { return (bool) json["IsPaid"]; }
			set { json["IsPaid"] = value; }
		}

		public DateTime Created
		{
			get { return (DateTime) json["Created"]; }
			set { json["Created"] = value; }
		}

		public DateTime Completed
		{
			get { return (DateTime) json["Completed"]; }
			set { json["Completed"] = value; }
		}

		public void AddItem(string name, int qty)
		{
			if (json["Items"] == null) json.Add("Items", new JArray());
			var jo = new JObject();
			var item = new Item(jo);
			item.Name = name;
			item.Qty = qty;

			((JArray) json["Items"]).Add(jo);
		}

		public DateTime TTL
		{
			get { return (DateTime) json["TTL"]; }
			set { json["TTL"] = value; }
		}

		public int Id
		{
			get { return this.OrderNumber; }
		}

		public class Item
		{
			private JToken json;

			public Item()
			{

			}

			public Item(JToken json)
			{
				this.json = json;
			}

			public string Name
			{
				get { return (string) json["Name"]; }
				set { json["Name"] = value; }
			}

			public int Qty
			{
				get { return (int) json["Qty"]; }
				set { json["Qty"] = value; }
			}

			public List<string> Ingredients
			{
				get { return json["Ingredients"].Select(t => t.ToString()).ToList(); }
				set { json["Ingredients"] = JToken.FromObject(value); }
			}

			public decimal Price
			{
				get { return (decimal) json["Price"]; }
				set { json["Price"] = value; }
			}


			public void AddTo(JObject jsonDocument)
			{

			}
		}

		[TestFixture]
		public class DocumentProblemTestFixture
		{
			[Test]
			public void CanReadOrderNumber()
			{
				var o = GetOrderDocument();
				Order order = new Order(o);
				Assert.That(order.OrderNumber, Is.EqualTo(1234));
			}

			[Test]
			public void CanReadItems()
			{
				var o = GetOrderDocument();
				Order order = new Order(o);
				Assert.That(order.Items.First().Name, Is.EqualTo("something"));
			}

			private JObject GetOrderDocument()
			{
				using (var stream = File.OpenText("OrderDocument.js"))
				{
					return (JObject) JObject.ReadFrom(new Newtonsoft.Json.JsonTextReader(stream));
				}
			}
		}
	}
}