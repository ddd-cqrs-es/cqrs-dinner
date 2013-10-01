namespace Dinner.Monitoring
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Messaging;

	public class QueueMonitor
	{
		private IEnumerable<IAmMonitored> handlers;

		public QueueMonitor(IEnumerable<IAmMonitored> handlers)
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
}