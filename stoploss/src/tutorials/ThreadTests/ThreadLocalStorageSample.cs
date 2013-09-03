namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class ThreadLocalStorageSample
	{
		[ThreadStatic] private static int _x = 3;

		[Test]
		public void Thread_static()
		{
			var t1 = new Thread(() =>
			{
				_x = _x + 1;
				Assert.That(_x, Is.EqualTo(1)); //Variable not inittialized to 3!
				Console.WriteLine("x: " + _x);
			});
			t1.Start();
			t1.Join();
			Assert.That(_x, Is.EqualTo(3));
		}

		private static readonly ThreadLocal<int> _y = new ThreadLocal<int>(() => 3);

		[Test]
		public void Thread_local()
		{
			var t1 = new Thread(() =>
			{
				_y.Value = _y.Value + 1;
				Assert.That(_y.Value, Is.EqualTo(4)); //Variable not inittialized to 3!
				Console.WriteLine("y: " + _y.Value);
			});
			t1.Start();
			t1.Join();
			Assert.That(_y.Value, Is.EqualTo(3));
		}

		[Test]
		public void Thread_local_variable()
		{
			var localRandom = new ThreadLocal<Random>(() => new Random());

			var t1 = new Thread(() =>
			{
				var x = localRandom.Value.Next();
				Console.WriteLine(x);
			});
			t1.Start();

			var y = localRandom.Value.Next();


		}


	}
}