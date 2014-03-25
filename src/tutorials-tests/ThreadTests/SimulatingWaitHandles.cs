namespace tutorials.ThreadTests
{
	using System.Threading;

	public class SimulatingWaitHandles
	{
		 
	}

	public class MyManualResetEvent
	{
		private readonly object locker = new object();
		private bool signal;

		public void WaitOne()
		{
			lock (locker)
			{
				while (!signal)
				{
					Monitor.Wait(locker);
				}
			}
		}

		public void Set(){
			lock (locker)
			{
				signal = true; 
				Monitor.PulseAll(locker);
			}
		}

		public void Reset()
		{
			lock (locker) signal = false;
		}
	}


	public class MyAutoResetEvent
	{
		private readonly object locker = new object();
		private bool signal;

		public void WaitOne()
		{
			lock (locker)
			{
				while (!signal)
				{
					Monitor.Wait(locker);
				}
				signal = false;
			}
		}

		public void Set()
		{
			lock (locker)
			{
				signal = true;
				Monitor.Pulse(locker);
			}
		}

		public void Reset()
		{
			lock (locker) signal = false;
		}
	}


}