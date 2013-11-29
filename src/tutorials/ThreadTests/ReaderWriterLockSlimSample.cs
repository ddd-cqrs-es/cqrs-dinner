namespace tutorials.ThreadTests
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class ReaderWriterLockSlimSample
	{
		ReaderWriterLockSlim rw = new ReaderWriterLockSlim();
		List<int> items = new List<int>();
		Random rnd = new Random();

		[Test]
		public void Main()
		{
			new Thread(Read).Start();
			new Thread(Read).Start();
			new Thread(Read).Start();
			new Thread(Write).Start("A");
			new Thread(Write).Start("B");

			Thread.Sleep(10000);

		}

		private void Write(object tid)
		{
			while (true)
			{
				Console.WriteLine(rw.CurrentReadCount + " concurrent readers");
				int newNumber = GetRandNum(100);
				rw.EnterWriteLock();
				items.Add(newNumber);
				rw.ExitWriteLock();
				Console.WriteLine("Thread "+ tid + " added " + newNumber);
			}			
		}

		private int GetRandNum(int max)
		{
			lock (rnd)
			{
				return rnd.Next(max);
			}
		}

		private void Read()
		{
			while (true)
			{
				rw.EnterReadLock();
				foreach (var item in items)
				{
					Thread.Sleep(10);
				}
				rw.ExitReadLock();
			}
		}

	}

	[TestFixture]
	public class ReaderWriterLockSlim_upgrade_sample
	{
		ReaderWriterLockSlim rw = new ReaderWriterLockSlim();
		List<int> items = new List<int>();
		Random rnd = new Random();

		[Test]
		public void Main()
		{
			new Thread(Read).Start();
			new Thread(Read).Start();
			new Thread(Read).Start();
			new Thread(Write).Start("A");
			new Thread(Write).Start("B");

			Thread.Sleep(10000);

		}

		private void Write(object tid)
		{
			while (true)
			{
				Console.WriteLine(rw.CurrentReadCount + " concurrent readers");
				int newNumber = GetRandNum(100);
				rw.EnterUpgradeableReadLock();
				if (!items.Contains(newNumber))
				{
					rw.EnterWriteLock();
					items.Add(newNumber);
					rw.ExitWriteLock();
					Console.WriteLine("Thread " + tid + " added " + newNumber);
				}
				rw.ExitUpgradeableReadLock();
			}
		}

		private int GetRandNum(int max)
		{
			lock (rnd)
			{
				return rnd.Next(max);
			}
		}

		private void Read()
		{
			while (true)
			{
				rw.EnterReadLock();
				foreach (var item in items)
				{
					Thread.Sleep(10);
				}
				rw.ExitReadLock();
			}
		}

	}

}