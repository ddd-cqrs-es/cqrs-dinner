namespace stoplosskata.lessstate
{
	using System.Linq;
	using NUnit.Framework;

	[TestFixture]
	public class MovingStoplossSpecs
	{
		[Test]
		public void When_position_is_acquired()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 15, Symbol = "ABC" });

			Assert.That(bus.PublishedMessages.Count, Is.EqualTo(0));
		}

		[Test]
		public void When_low_price_is_sustained_below_trigger_we_sell()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 1, Symbol = "ABC" });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.89m });
			var first = (WakeMeUpIn15Seconds)bus.PublishedMessages.First();
			bus.PublishedMessages.Clear();
			processManager.Handle((ShouldWeSell)first.Message);

			Assert.That(bus.SentMessages.Count, Is.EqualTo(1));

		}


		[Test]
		public void When_low_price_is_sustained_below_trigger_we_sell_once()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 1, Symbol = "ABC" });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.89m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.88m });

			var first = (WakeMeUpIn15Seconds)bus.PublishedMessages[0];
			var second = (WakeMeUpIn15Seconds)bus.PublishedMessages[1];

			bus.PublishedMessages.Clear();
			processManager.Handle((ShouldWeSell)first.Message);
			processManager.Handle((ShouldWeSell)second.Message);

			Assert.That(bus.SentMessages.Count, Is.EqualTo(1));

		}


		[Test]
		public void When_low_price_is_not_sustained_below_trigger_we_do_nothing()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 1, Symbol = "ABC" });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.89m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.90m });

			var first = (WakeMeUpIn15Seconds)bus.PublishedMessages.First();
			bus.PublishedMessages.Clear();
			processManager.Handle((ShouldWeSell)first.Message);

			Assert.That(bus.SentMessages.Count, Is.EqualTo(0));

		}



		[Test]
		public void When_high_price_is_sustained_trigger_value_goes_up()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 1, Symbol = "ABC" });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.01m });

			var fisrt = (WakeMeUpIn15Seconds)bus.PublishedMessages[0];
			var second = (WakeMeUpIn20Seconds)bus.PublishedMessages[1];

			bus.PublishedMessages.Clear();

			processManager.Handle((ShouldWeSell)fisrt.Message);
			processManager.Handle((ShouldWeMoveTriggerPrice)second.Message);

			Assert.That(bus.PublishedMessages.Count, Is.EqualTo(1));
			var triggerValueRaised = (TriggerValueRaised)bus.PublishedMessages[0];
			Assert.That(triggerValueRaised.TriggerValue, Is.EqualTo(0.91m));
		}

		[Test]
		public void When_high_price_is_sustained_with_a_dip_trigger_value_goes_up()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 1, Symbol = "ABC" });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.1m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.2m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.15m });


			var fisrt = (WakeMeUpIn15Seconds)bus.PublishedMessages[0];
			var second = (WakeMeUpIn20Seconds)bus.PublishedMessages[1];

			bus.PublishedMessages.Clear();

			processManager.Handle((ShouldWeSell)fisrt.Message);
			processManager.Handle((ShouldWeMoveTriggerPrice)second.Message);

			Assert.That(bus.PublishedMessages.Count, Is.EqualTo(1));
			var triggerValueRaised = (TriggerValueRaised)bus.PublishedMessages[0];
			Assert.That(triggerValueRaised.TriggerValue, Is.EqualTo(1.0m));
		}



		[Test]
		public void When_low_price_is_sustained_with_a_small_spike_still_sells()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 1, Symbol = "ABC" });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.89m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.8m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.85m });


			var fisrt = (WakeMeUpIn15Seconds)bus.PublishedMessages[0];

			var second = (WakeMeUpIn15Seconds)bus.PublishedMessages[1];

			bus.PublishedMessages.Clear();

			processManager.Handle((ShouldWeSell)fisrt.Message);
			processManager.Handle((ShouldWeSell)second.Message);

			Assert.That(bus.SentMessages.Count, Is.EqualTo(1));
			var sellPosition = (SellPosition)bus.SentMessages[0];
			Assert.That(sellPosition.Price, Is.EqualTo(0.89m));
		}

		[Test]
		public void When_value_many_dips_occur_only_lastsustained_price_isaccounted_for()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 1, Symbol = "ABC" });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.1m }); //this price holds
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.2m }); //does not hold
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.15m }); //holds
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.25m }); //holds
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.30m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.27m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.27m });
			
			var wakeMeUpIn20Secondses = bus.PublishedMessages.OfType<WakeMeUpIn20Seconds>().Take(4).ToArray();

			bus.PublishedMessages.Clear();

			//processManager.Handle((ShouldWeSell)fisrt.Message);
			foreach (var wakeMeUpIn20Seconds in wakeMeUpIn20Secondses)
			{
				processManager.Handle((ShouldWeMoveTriggerPrice)wakeMeUpIn20Seconds.Message);				
			}

			Assert.That(bus.PublishedMessages.Count, Is.EqualTo(3));
			var triggerValueRaised = (TriggerValueRaised)bus.PublishedMessages[0];
			Assert.That(triggerValueRaised.TriggerValue, Is.EqualTo(1m));

			var triggerValueRaised2 = (TriggerValueRaised)bus.PublishedMessages[1];
			Assert.That(triggerValueRaised2.TriggerValue, Is.EqualTo(1.05m));

			var triggerValueRaised3 = (TriggerValueRaised)bus.PublishedMessages[2];
			Assert.That(triggerValueRaised3.TriggerValue, Is.EqualTo(1.15m));
		}


		[Test]
		public void When_high_price_is_not_sustained_trigger_stays_the_same()
		{
			var bus = new FakeBus();
			var processManager = new StoplossProcess(bus);
			processManager.Handle(new PositionAcquired { Price = 1, Symbol = "ABC" });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 1.01m });
			processManager.Handle(new PriceChanged { Symbol = "ABC", Price = 0.99m });

			var fisrt = (WakeMeUpIn15Seconds)bus.PublishedMessages[0];
			var second = (WakeMeUpIn20Seconds)bus.PublishedMessages[1];
			

			bus.PublishedMessages.Clear();

			processManager.Handle((ShouldWeSell)fisrt.Message);
			processManager.Handle((ShouldWeMoveTriggerPrice)second.Message);

			Assert.That(bus.PublishedMessages.Count, Is.EqualTo(0));
		}


	}
}