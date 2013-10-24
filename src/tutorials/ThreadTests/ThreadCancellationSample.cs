namespace tutorials.ThreadTests
{
	using System;
	using System.Runtime.InteropServices;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class ThreadCancellationSample
	{

		[Test]
		public void Cancel_with_ruly_canceller()
		{
			var canceler = new RulyCanceler();
			new Thread(()=>
			{
				try
				{
					Work(canceler);
				}catch(OperationCanceledException){Console.WriteLine("Canceled!");}
			}).Start();

			canceler.Cancel();
		}

		private void Work(RulyCanceler canceler)
		{
			while (true)
			{
				canceler.ThrowIfCancellationRequested();

				try
				{
					OtherMethod(canceler);
				}
				finally
				{
					//cleanup resources
				}
				
			}
		}

		private void OtherMethod(RulyCanceler canceler)
		{
			canceler.ThrowIfCancellationRequested();
		}
	}


	public class RulyCanceler
	{
		readonly object cancelLocker = new object();
		private bool cancelRequest;

		public bool IsCancellationRequested
		{
			get
			{
				lock (cancelLocker)
				{
					return cancelRequest;
				}
			}
		}

		public void Cancel()
		{
			lock (cancelLocker)
			{
				cancelRequest = true;
			}
		}

		public void ThrowIfCancellationRequested()
		{
			if (IsCancellationRequested) { throw new OperationCanceledException(); }
		}

	}
}