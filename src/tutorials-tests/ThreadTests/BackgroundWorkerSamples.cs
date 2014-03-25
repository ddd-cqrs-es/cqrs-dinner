namespace tutorials.ThreadTests
{
	using System;
	using System.ComponentModel;
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class BackgroundWorkerSamples
	{
		private static readonly BackgroundWorker bw = new BackgroundWorker();

		[Test]
		public void Start_a_background_worker()
		{
			bw.DoWork += bw_DoWork;
			bw.RunWorkerAsync("Message to worker");

			while (bw.IsBusy)
			{
			}
		}

		private void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			Console.WriteLine(e.Argument);
		}

		[Test]
		public void Background_worker_throws_excepiton()
		{
			bw.DoWork += bw_ThrowException;
			Exception e = null;
			bw.RunWorkerCompleted += (sender, args) => e = args.Error;

			bw.RunWorkerAsync();
			//spin while not completed
			while (bw.IsBusy)
			{
				Thread.Sleep(10); //wait for worker completed finished
			}

			Assert.That(e, Is.InstanceOf<FoobarException>());
		}

		private void bw_ThrowException(object sender, DoWorkEventArgs e)
		{
			throw new FoobarException();
		}

		[Test]
		public void Backgrounf_worker_cancellation_and_progress()
		{
			var backgroundWorker = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};

			backgroundWorker.DoWork += (sender, args) =>
			{
				for (var i = 0; i < 100; i += 20)
				{
					if (backgroundWorker.CancellationPending)
					{
						args.Cancel = true;
						return;
					}
					backgroundWorker.ReportProgress(i);
					Thread.Sleep(1000);
				}

				args.Result = 123;
			};

			backgroundWorker.ProgressChanged += (sender, args) => Console.WriteLine("Reached " + args.ProgressPercentage + "%");
			backgroundWorker.RunWorkerCompleted += (sender, args) =>
			{
				if (args.Cancelled)
				{
					Console.WriteLine("You cancelled!");
				}
				else if (args.Error != null)
				{
					Console.WriteLine("Worker Exception: " + args.Error.ToString());
				}
				else
				{
					Console.WriteLine("Completed: " + args.Result);
				}
			};

			backgroundWorker.RunWorkerAsync("Hello to worker");

			Thread.Sleep(2000);
			if(backgroundWorker.IsBusy) backgroundWorker.CancelAsync();

			Thread.Sleep(1100); //wait for last loop

		}

		[Test]
		public void Background_worker_subclass_does_EAP()
		{
			var financialWorker = new FinancialWorker(10, 20);

			financialWorker.ProgressChanged+= (sender, args) => Console.WriteLine("Progress: " +args.ProgressPercentage);
			financialWorker.RunWorkerCompleted += (sender, args) => Console.WriteLine("Completed: " + args.Result);

			financialWorker.RunWorkerAsync();

			while (financialWorker.IsBusy)
			{
				Thread.Sleep(10); //wait for worker completed finished
			}

		}
	}

	public class FinancialWorker:BackgroundWorker
	{
		private int Foo;
		private int Bar;

		public FinancialWorker(int foo, int bar)
		{
			WorkerReportsProgress = true;
			WorkerSupportsCancellation = true;
			this.Foo = foo;
			this.Bar = bar;
		}

		protected override void OnDoWork(DoWorkEventArgs e)
		{
			ReportProgress(0, "Working hard for the money...");

			bool finishedReport = false;
			int progress = 0;
			while (progress <100)
			{
				if(CancellationPending)
				{
					e.Cancel = true; return;
					
				}

				Thread.Sleep(1000);

				ReportProgress(progress += 10, "Getting there...");

			}

			ReportProgress(100);
		}
	}

	internal class FoobarException : Exception
	{
	}
}