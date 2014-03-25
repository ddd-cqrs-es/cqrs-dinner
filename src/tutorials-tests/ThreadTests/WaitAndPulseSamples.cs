namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class WaitAndPulseSamples
	{
		private readonly object locker = new object();
		private bool go;

		[Test]
		public void Main()
		{
			new Thread(Work).Start(); //block because go == false

			//simulate waiting for user
			var rnd = new Random((int) DateTime.Now.Ticks);
			Thread.Sleep((int) (rnd.NextDouble() * 10000));

			lock (locker) //Wake up the thread by setting go and pulsing
			{
				go = true;
				Monitor.Pulse(locker);
			}
		}

		private void Work()
		{
			lock (locker)
			{
				while (!go)
				{
					Monitor.Wait(locker); //lock is released
					//lock reaquired once puled
				}
			}

			Console.WriteLine("Woken!!!");
		}
	}

	
}