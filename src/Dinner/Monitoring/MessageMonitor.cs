namespace Dinner.Monitoring
{
	using System;
	using Messaging;

	public class 
		MessageMonitor : IHandle<IMessage>, IHandle<OrderPlaced>, IHandle<DodgyOrderPlaced>, IHandle<OrderCompleted>
	{
		private readonly Dispatcher d;

		public MessageMonitor(Dispatcher d)
		{
			this.d = d;
		}

		public void Handle(IMessage message)
		{
			Console.WriteLine("CoId:{2}  Message:{3}\t Id:{0} CaId:{1}", message.Id, message.CausationId, message.CorolationId, message);
		}

		public void Handle(OrderPlaced message)
		{
			d.Subscribe<IMessage>(this, message.CorolationId.ToString());
			d.Subscribe<OrderCompleted>(this, message.CorolationId.ToString());
		}

		public void Handle(DodgyOrderPlaced message)
		{
			d.Subscribe<IMessage>(this, message.CorolationId.ToString());
			d.Subscribe<OrderCompleted>(this, message.CorolationId.ToString());

		}

		public void Handle(OrderCompleted message)
		{
			d.Unsubscribe<IMessage>(this, message.CorolationId.ToString());
		}
	}
}