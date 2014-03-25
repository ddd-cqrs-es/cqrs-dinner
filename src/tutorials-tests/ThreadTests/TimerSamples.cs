namespace tutorials.ThreadTests
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class TimerSamples
	{
		private ConcurrentStack<string> data = new ConcurrentStack<string>();

		[Test]
		public void Simple_timer_ticks_every_5_s()
		{
			var timer = new Timer(Tick, "tick...", 5000, 1000);
			Thread.Sleep(9500);

			Assert.That(data.Count, Is.EqualTo(5));

		}

		private void Tick(object state)
		{
			Console.WriteLine(state);
			data.Push((string) state);
			Thread.Sleep(10000);
		}
	}
}