namespace stoplosskata
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using NUnit.Framework;

	public abstract class Specification
	{
		protected abstract IEnumerable<Message> Given();
		private List<Message> then;

		[TestFixtureSetUp]
		public void TestfixtureSetUp()
		{
			then = new List<Message>();

			var sut = new Stoploss();

			foreach (var message in Given())
			{
				var consume = typeof (IConsume<>).MakeGenericType(message.GetType()).GetMethod("Consume");
				consume.Invoke(sut, new object[] {message});
			}
			
			then.AddRange(sut.ProducedMessages);

		}

		protected IEnumerable<Message> Then
		{
			get { return then; }
		}
	}

	[TestFixture]
	public class When_a_trailing_position_is_taken : Specification
	{
		private Guid positionId;

		protected override IEnumerable<Message> Given()
		{
			positionId = Guid.NewGuid();
			yield return new PositionAcquiredMessage { Price = 10.0, PositionId = positionId, StopLoss = 1.0 };
			var priceChangedMessage = new PriceChangedMessage {Price = 11.0};
			yield return priceChangedMessage;
			yield return new Elapsed15Sec{ChangeId = priceChangedMessage.Id};
			yield return new PriceChangedMessage { Price = 10.0 };

		}

		
		[Test]
		public void Zero_message_produced()
		{
			Then.CountIs(0);
		}

	}


	[TestFixture]
	public class When_a_position_is_taken: Specification
	{
		private Guid positionId;

		protected override IEnumerable<Message> Given()
		{
			positionId = Guid.NewGuid();
			yield return new PositionAcquiredMessage{Price = 10.0, PositionId = positionId, StopLoss = 1.0};
			yield return new PriceChangedMessage{Price= 9.0}; 

		}

		[Test]
		public void Release_position_sent()
		{
			Then.ContainsMessage(new SellPositionMessage {PositionId = positionId});
		}

		[Test]
		public void Only_one_mmessage_produced()
		{
			Then.CountIs(1);
		}

	}

	public static class TestExtension
	{
		public static void ContainsMessage<TMessage>(this IEnumerable<Message> messages, TMessage exprected) where TMessage : Message
		{
			CollectionAssert.Contains(messages, exprected);
		}

		public static void CountIs(this IEnumerable<Message> messages,int exprected)
		{
			Assert.That(messages.Count(), Is.EqualTo(exprected));
			
		}
	}

}