namespace tutorials.ThreadTests
{
	using System;
	using System.IO;
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
 
		[Test]
		public void Thread_still_aborts_if_caught()
		{
			var t = new Thread(CatchThreadAbort);
			t.Start();
			t.Abort();
			t.Join();

			Assert.That(t.ThreadState == ThreadState.Aborted);
		}

		private void CatchThreadAbort()
		{
			while (true)
			{
				try
				{
					while (true)
					{

					}
				}
				catch (ThreadAbortException threadAbortException)
				{
					//do something usefull	
				}
			}
		}

		[Test]
		public void Thread_wont_die()
		{
			var t = new Thread(IWillNotDie);
			t.Start();
			
			Thread.Sleep(1000);
			t.Abort();
			Thread.Sleep(1000);
			t.Abort();
			Thread.Sleep(1000);
			t.Abort();
			
			Assert.That(t.ThreadState == ThreadState.Running);
				
		}

		private void IWillNotDie()
		{
			while (true)
			{
				try
				{
					while (true)
					{

					}
				}
				catch (ThreadAbortException threadAbortException)
				{
					Thread.ResetAbort();
					Console.WriteLine("I wwill not die!");
				}
			}
		}

		[Test]
		public void Thread_abort_complication()
		{
			turnstyle = new AutoResetEvent(false);
			var t = new Thread(Complicated);

			t.Start();

			t.Abort();

			t.Join();

			Assert.That(IsOpen(), Is.True);

			
		}

		[TearDown]
		public void CloseFile()
		{
			if (sw != null)
			{
				sw.Dispose();
			}
		}

		private bool IsOpen()
		{
			try
			{
				File.Open("myfile.txt", FileMode.Open);
			}
			catch (IOException e)
			{
				return true;
			}
			return false;
		}

		private AutoResetEvent turnstyle;
		private StreamWriter sw;

		private void Complicated()
		{
			//Equivalent to:
			sw = File.CreateText("myfile.txt");
			turnstyle.WaitOne();
			// if we get a signal tp abort the  file never gets closed
			try{ sw.Write("About-safe?");}
			finally{ sw.Dispose(); }
			
//			using (var sw = File.CreateText("myfile.txt"))
//			{
//				sw.Write("About-safe?");
//			}
		}
	}
}