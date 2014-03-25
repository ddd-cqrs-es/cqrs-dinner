namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;

	public class MemoryBarriersSample
	{
		 public class Foo
		 {
			 private int answer;
			 private bool complete;

			 public void A()
			 {
				 answer = 123;
				 Thread.MemoryBarrier();
				 complete = true;
				 Thread.MemoryBarrier();


			 }

			 public void B()
			 {
				 Thread.MemoryBarrier();
				 if (complete)
				 {
					 Thread.MemoryBarrier();
					 Console.WriteLine(answer);
				 }
			 }

		 }
	}

	public class VolatileSample
	{
		public class Foo
		{
			private int answer;
			private volatile bool complete;

			public void A()
			{
				answer = 123;
				complete = true;
				

			}

			public void B()
			{
				if (complete)
				{
					Console.WriteLine(answer);
				}
			}

		}
	}
}