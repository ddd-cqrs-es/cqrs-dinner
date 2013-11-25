﻿namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class SimulatingCountDownEvent
	{
		static MyCountdownEvent countdown = new MyCountdownEvent(3);

		[Test]
		public void Countdown_event()
		{
			new Thread(SaySomething).Start("I am thread 1");
			new Thread(SaySomething).Start("I am thread 2");
			new Thread(SaySomething).Start("I am thread 3");

			countdown.Wait(); //blocks unitil signaled three times
			Console.WriteLine("All threads have finished speaking!");

		}

		private void SaySomething(object thingy)
		{
			Thread.Sleep(1000);
			Console.WriteLine(thingy);
			countdown.Signal();
		}

	 

	}

	public class MyCountdownEvent
	{
		object locker = new object();
		private int value;

		public MyCountdownEvent(){}
		public MyCountdownEvent(int initialcount)
		{
			value = initialcount;
		}

		public void AddCount(int amount)
		{
			lock (locker)
			{
				value += amount;
				if(value <=0) Monitor.PulseAll(locker);
			}
		}

		public void Signal(){AddCount(-1);}

		public void Wait(){
			lock (locker)
			{
				while (value > 0) Monitor.Wait(locker);
			}
		}
	}
}