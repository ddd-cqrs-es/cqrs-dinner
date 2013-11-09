namespace tutorials.ThreadTests
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class WaitPulseQueueSample
	{
		[Test]
		public void Main()
		{
			var queue = new WaitPulseProducerConsumerQueue(3);
			var random = new Random(DateTime.Now.Millisecond);
			Console.WriteLine("Enqueuing 10 items");
			for (var i = 0; i < 100; i++)
			{
				var itemNumber = i;
				queue.EnqueueItem(() =>
					{
						var millisecondsTimeout = (itemNumber%5)*1000;
						Thread.Sleep(millisecondsTimeout);
						Console.WriteLine(" Task " + itemNumber + " slept " + millisecondsTimeout + " ms");
					});
			}

			queue.Shutdown(true);

			Console.WriteLine("Worker complete!");
		}
	}

	public class WaitPulseProducerConsumerQueue
	{
		private readonly Thread[] workers;
		private readonly Queue<Action> queue = new Queue<Action>();
		private readonly object locker = new object();

		public WaitPulseProducerConsumerQueue(int workerCount)
		{
			workers = new Thread[workerCount];

			for (var i = 0; i < workerCount; i++)
			{
				var thread = new Thread(Consume);
				thread.Start(i);
				workers[i] = thread;
			}
		}

		private void Consume(object data)
		{
			while (true)
			{
				Action action;
				lock (locker)
				{
					while (queue.Count == 0)
					{
						Monitor.Wait(locker); //this makes cunsumer pull  
					}
					action = queue.Dequeue();
				}
				if (action == null)
				{
					return;
				}
				Console.Write("Consumer " + data + " ");
				action(); //invoke action
			}
		}

		public void Shutdown(bool waitForWorker)
		{
			foreach (var worker in workers)
			{
				EnqueueItem(null);
			}

			if (waitForWorker)
			{
				foreach (var worker in workers)
				{
					worker.Join();
				}
			}
		}

		public void EnqueueItem(Action action)
		{
			lock (locker)
			{
				queue.Enqueue(action);
				Monitor.Pulse(locker);
			}
		}
	}
}