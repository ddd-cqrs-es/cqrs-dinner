namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;
	using NUnit.Framework;

	public class TwoWaySignaling
	{
		 
	}

	[TestFixture]
	public class Race
	{
		private static readonly object locker = new object();
		private static bool go;


		[Test]
		public void Main()
		{
			new Thread(SaySomething).Start();

			for (int i = 0; i < 5; i++)
			{
				lock (locker)
				{
					go = true;
					Monitor.PulseAll(locker);
				}
			}

		}

		private void SaySomething()
		{
			for (int i = 0; i < 5; i++)
			{
				lock (locker)
				{
					while (!go) Monitor.Wait(locker);
					go = false;
					Console.WriteLine("Wassup?");

				}
			}
		}
	}

	[TestFixture]
	public class NoRace
	{
		private static readonly object locker = new object();
		private static bool go;
		private static bool ready;

		[Test]
		public void Main()
		{
			new Thread(SaySomething).Start();

			for (int i = 0; i < 5; i++)
			{
				lock (locker)
				{
					while (!ready) Monitor.Wait(locker);
					ready = false;
					go = true;
					Monitor.PulseAll(locker);
				}
			}

		}

		private void SaySomething()
		{
			for (int i = 0; i < 5; i++)
			{
				lock (locker)
				{
					ready = true;
					Monitor.PulseAll(locker);
					while (!go) Monitor.Wait(locker);
					go = false;
					Console.WriteLine("Wassup?");

				}
			}
		}
	}

}