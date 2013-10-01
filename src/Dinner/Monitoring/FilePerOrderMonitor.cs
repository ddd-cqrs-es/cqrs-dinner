namespace Dinner.Monitoring
{
	using System;
	using System.Collections.Concurrent;
	using System.IO;
	using Messaging;

	public class
		FilePerOrderMonitor : IHandle<IMessage>, IHandle<OrderPlaced>, IHandle<DodgyOrderPlaced>, IHandle<OrderCompleted>
	{
		private readonly Dispatcher d;
		private readonly ConcurrentDictionary<Guid, StreamWriter> files = new ConcurrentDictionary<Guid, StreamWriter>();
	
		public FilePerOrderMonitor(Dispatcher d)
		{
			this.d = d;
		}

		public void Handle(IMessage message)
		{
			StreamWriter sw;

			while (!files.TryGetValue(message.CorolationId, out sw))
			{
				
			}
			;
			sw.WriteLine("CoId:{2}  Message:{3}\t Id:{0} CaId:{1}", message.Id, message.CausationId, message.CorolationId, message);
		}

		public void Handle(OrderPlaced message)
		{
			string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dinner");
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			
			string fileName = "Dinner\\" + message.CorolationId.ToString() + ".log";
			var fileStream =
				new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName), false);

			while (!files.TryAdd(message.CorolationId, fileStream))
			{
				
			}
			
			d.Subscribe<IMessage>(this, message.CorolationId.ToString());
			d.Subscribe<OrderCompleted>(this, message.CorolationId.ToString());
		}

		public void Handle(DodgyOrderPlaced message)
		{
			string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dinner");
			if(!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			string fileName = "Dinner\\" + message.CorolationId.ToString() + ".log";
			var fileStream =
				new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName), false);

			while (!files.TryAdd(message.CorolationId, fileStream))
			{
				
			}
			

			d.Subscribe<IMessage>(this, message.CorolationId.ToString());
			d.Subscribe<OrderCompleted>(this, message.CorolationId.ToString());

		}

		public void Handle(OrderCompleted message)
		{
			StreamWriter streamWriter;
			files.TryRemove(message.CorolationId,out streamWriter);
			streamWriter.Close();
			d.Unsubscribe<IMessage>(this, message.CorolationId.ToString());
		}
	}
}