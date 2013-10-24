namespace tutorials.ThreadTests
{
	using System;
	using System.Threading;
	using NUnit.Framework;

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