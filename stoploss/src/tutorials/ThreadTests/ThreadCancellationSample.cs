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

	[TestFixture]
	public class Cancellation_token_sample
	{
		[Test]
		public void Use_it()
		{
			var cancelSource = new CancellationTokenSource();

			var t = new Thread(() => Work(cancelSource.Token));
			t.Start();

			Thread.Sleep(2000);

			cancelSource.Cancel();

			t.Join();
			
		}

		private void Work(CancellationToken token)
		{
			while (true)
			{
				if (token.IsCancellationRequested)
				{

					Console.WriteLine("Cancelling");
					
					//clean up
					token.ThrowIfCancellationRequested();
				}

				Thread.Sleep(1000);
			}

			Console.WriteLine("Cant get here");
		}
	}

}