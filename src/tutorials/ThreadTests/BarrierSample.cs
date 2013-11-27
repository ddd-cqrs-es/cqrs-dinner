namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class BarrierSample
	{
		private Barrier barrier;

		[Test]
		public void Main()
		{
			barrier = new Barrier(3);
			new Thread(Speak).Start("A");
			new Thread(Speak).Start("B");
			new Thread(Speak).Start("C");


		}

		private void Speak(object id)
		{
			for (int i = 0; i < 5; i++)
			{
				Console.Write(id.ToString() + i + " ");
				barrier.SignalAndWait();
			}
		}

		[Test]
		public void Post_phase_action()
		{
			barrier = new Barrier(3, x=> Console.WriteLine());
			new Thread(Speak).Start("A");
			new Thread(Speak).Start("B");
			new Thread(Speak).Start("C");


		}

	}
}