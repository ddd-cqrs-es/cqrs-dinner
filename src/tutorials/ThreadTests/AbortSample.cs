namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class AbortSample
	{
		[Test]
		public void Main()
		{
			var t = new Thread(() =>
				{ while (true) ; });
			Console.WriteLine(t.ThreadState); //unstarted
			t.Start();
			Thread.Sleep(1000);
			Console.WriteLine(t.ThreadState); // runnning

			t.Abort();
			Console.WriteLine(t.ThreadState); //Abort requested

			t.Join();
			Console.WriteLine(t.ThreadState); //Aborted
		}
 
	}
}