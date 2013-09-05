namespace tutorials.ThreadTests
{
	using System;
	using System.Net;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class EventBasedAsyncTests
	{
		[Test]
		public void Downlaod_async()
		{
			var wc = new WebClient();
			wc.DownloadStringCompleted += (sender, args) =>
			{
				if (args.Cancelled) Console.WriteLine("Cancelled");
				else if (args.Error != null) Console.WriteLine("Exception: " + args.Error.Message);
				else
				{
					Console.WriteLine("Downloaded: " + args.Result.Substring(0, 80) + "...");
				}
			};

			wc.DownloadStringAsync(new Uri("http://www.linqpad.com"));

			Thread.Sleep(1000);
		} 
	}
}