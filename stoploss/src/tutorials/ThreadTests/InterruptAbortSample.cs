namespace tutorials.ThreadTests
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class InterruptAbortSample
	{
		[Test]
		public void Interrupt_thread()
		{
			var t = new Thread(() =>
			{
				try
				{
					Thread.Sleep(Timeout.Infinite);
				}
				catch (ThreadInterruptedException e)
				{
					Console.Write("Forcibly ...");

				}
				Console.WriteLine("Woken!");
			});

			t.Start();
			t.Interrupt();
		}

		[Test]
		public void Interrupt_thread_does_not_throw_if_not_blocked()
		{
			bool threadInterrupted = false;
			var t = new Thread(() =>
			{
				try
				{
					
					for (int i = 0; i < 1000; i++)
					{
						Console.Write(i);
					}
				}
				catch (ThreadInterruptedException e)
				{
					threadInterrupted = true;
					Console.Write("Forcibly ...");

				}
				Console.WriteLine("Woken!");
			});

			t.Start();
			t.Interrupt();
			t.Join();
			Assert.That(threadInterrupted, Iz.False);
		}

		[Test]
		public void Interrupt_thread_does_throws_when_blocked()
		{
			bool threadInterrupted = false;
			var counter = 0;
			var t = new Thread(() =>
			{
				try
				{

					for (int i = 0; i < 1000; i++)
					{
						counter = i;
						Console.Write(i);
					}
					Thread.Sleep(Timeout.Infinite);
				}
				catch (ThreadInterruptedException e)
				{
					threadInterrupted = true;
					Console.Write("Forcibly ...");

				}
				Console.WriteLine("Woken!");
			});

			t.Start();
			t.Interrupt();
			t.Join();
			Assert.That(threadInterrupted, Iz.True);
			Assert.That(counter, Is.EqualTo(999));
		}
	}
}