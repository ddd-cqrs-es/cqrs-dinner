namespace Dinner
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using NUnit.Framework;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

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
			get { return (int)json["TableNumber"]; }
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