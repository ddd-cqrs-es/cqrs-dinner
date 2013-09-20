namespace Dinner
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using NUnit.Framework;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

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
			foreach (var handler in messageHandlers)
			{
				handler.Handle(message);
			}
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

	public class Monitor2 : IHandle<IMessage>, IHandle<OrderPlaced>
	{
		private readonly Dispatcher d;

		public Monitor2(Dispatcher d)
		{
			this.d = d;
		}

		public void Handle(IMessage message)
		{
			Console.WriteLine("Id:{0} CaId:{1} CoId:{2} {3}", message.Id, message.CausationId, message.CorolationId, message);
		}

		public void Handle(OrderPlaced message)
		{
			d.Subscribe<IMessage>(this, message.CorolationId.ToString());
		}

		public void Handle(OrderPaid message)
		{
			d.Unsubscribe<IMessage>(this, message.CorolationId.ToString());
		}
	}

	public class Narrower<TInput, TOutput> : IHandle<TInput>, IEquatable<Narrower<TInput, TOutput>> where TOutput : TInput
	{
		public bool Equals(Narrower<TInput, TOutput> other)
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
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Narrower<TInput, TOutput>) obj);
		}

		public override int GetHashCode()
		{
			return handler.GetHashCode();
		}

		private readonly IHandle<TOutput> handler;

		public Narrower(IHandle<TOutput> handler)
		{
			this.handler = handler;
		}

		public void Handle(TInput message)
		{
			try
			{
				handler.Handle((TOutput) message);
			}
			catch (InvalidCastException)
			{
				
			}
		}

		private TOutput ChangeType(TInput message)
		{
			try
			{
				return (TOutput) message;
			}
			catch
			{
				return default(TOutput);
			}
		}
	}

	public class Widener<T, U> : IHandle<T> where T : U
	{
		private readonly IHandle<U> handler;

		public Widener(IHandle<U> handler)
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			handler.Handle(message);
		}
	}

	public class Dispatcher
	{
		private readonly Dictionary<string, Multiplexer<IMessage>> topics = new Dictionary<string, Multiplexer<IMessage>>();

		public void Publish<T>(string topic, T message) where T : IMessage
		{
			if (topics.ContainsKey(topic))
			{
				topics[topic].Handle(message);
			}
			var corolationTopic = message.CorolationId.ToString();
			if(topics.ContainsKey(corolationTopic))
			topics[corolationTopic].Handle(message);
			//new Widener<T, IMessage>(topics[topic]).Handle(message);
			//new Widener<T, IMessage>(topics[message.CorolationId.ToString()]).Handle(message);
		}

		public void Publish<T>(T message) where T : IMessage
		{
			Publish(message.GetType().Name, message);
		}

		public void Subscribe<T>(IHandle<T> handler) where T : IMessage
		{
			Subscribe(handler, typeof (T).Name);
		}

		public void Subscribe<T>(IHandle<T> handler, string topic) where T : IMessage
		{
			if (!topics.ContainsKey(topic))
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

	public class Monitor
	{
		private IEnumerable<IAmMonitored> handlers;

		public Monitor(IEnumerable<IAmMonitored> handlers)
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
					Console.WriteLine("Queue {0} count {1}", queuedHandler.Name, queuedHandler.Count());
				}
				Thread.Sleep(1000);
			}
		}
	}

	public interface IAmMonitored
	{
		string Name { get; }
		int Count();
	}

	public class QueuedHandler<T> : IAmMonitored, IHandle<T> where T : IMessage
	{
		private ConcurrentQueue<T> queuedMessages = new ConcurrentQueue<T>();
		private IHandle<T> handler;
		private readonly string name;
		private volatile bool canceled;

		public QueuedHandler(IHandle<T> handler, string name)
		{
			this.handler = handler;
			this.name = name;
		}

		public string Name
		{
			get { return name; }
		}

		public int Count()
		{
			return queuedMessages.Count;
		}

		public void Start()
		{
			var t = new Thread(Run);
			t.Start();
		}

		private void Run()
		{
			while (!canceled)
			{
				T message;
				queuedMessages.TryDequeue(out message);
				if (message != null)
				{
					handler.Handle(message);
				}
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
			return message.TTL < DateTime.Now;
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

	public class Limiter<T> : IHandle<T> where T : IMessage
	{
		private readonly QueuedHandler<T> handler;

		public Limiter(QueuedHandler<T> handler)
		{
			this.handler = handler;
		}

		public void Handle(T message)
		{
			if (handler.Count() > 10000)
			{
				Console.Write(".");
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
		Guid Id { get; }
		Guid CausationId { get; }
		Guid CorolationId { get; }
	}

	public interface IHandle<T>
	{
		void Handle(T message);
	}

	public class Order
	{
		private JObject json;

		public Order() : this(new JObject())
		{
		}

		public Order(JObject json)
		{
			this.json = json;
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
			if (json["Items"] == null)
			{
				json.Add("Items", new JArray());
			}
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

		public Guid Id
		{
			get { return new Guid((string) json["Id"]); }
			set { json["Id"] = value.ToString(); }
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
				var order = new Order(o);
				Assert.That(order.Id, Is.EqualTo(1234));
			}

			[Test]
			public void CanReadItems()
			{
				var o = GetOrderDocument();
				var order = new Order(o);
				Assert.That(order.Items.First().Name, Is.EqualTo("something"));
			}

			private JObject GetOrderDocument()
			{
				using (var stream = File.OpenText("OrderDocument.js"))
				{
					return (JObject) JToken.ReadFrom(new JsonTextReader(stream));
				}
			}
		}
	}
}