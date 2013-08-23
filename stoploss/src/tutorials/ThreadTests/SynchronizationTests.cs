namespace tutorials.ThreadTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using ThreadState = System.Threading.ThreadState;

    public class ThreadUnsafe
    {
        private static int val1 = 1;
        private static int val2 = 1;
        private static bool firstPass=true;

        public static void Go()
        {
            if (val2 != 0)
            {
                if (firstPass) { firstPass = false; Thread.Sleep(10); }

                
                Console.WriteLine(val1/val2);
                val2 = 0;
            }
        }
    }

    public class ThreadSafe
    {
        private static int val1 = 1;
        private static int val2 = 1;
        private static bool firstPass = true;
        private static readonly object locker = new object();

        public static void Go()
        {
            lock (locker)
            {
                if (val2 != 0)
                {
                    if (firstPass)
                    {
                        firstPass = false;
                        Thread.Sleep(10);
                    }


                    Console.WriteLine(val1/val2);
                    val2 = 0;
                }
            }
        }
    }

    public class ThreadSafeMonitor
    {
        private static int val1 = 1;
        private static int val2 = 1;
        private static bool firstPass = true;
        private static readonly object locker = new object();

        public static void Go()
        {
            bool lockTaken = false;
            try
            {

                Monitor.Enter(locker, ref lockTaken);

                if (val2 != 0)
                {
                    if (firstPass)
                    {
                        firstPass = false;
                        Thread.Sleep(10);
                    }


                    Console.WriteLine(val1/val2);
                    val2 = 0;
                }
            }
            finally
            {
                if(lockTaken){Monitor.Enter(locker);}
            }
        }
    }


    [TestFixture]
    public class SynchronizationTests
    {
        [Test]
        public void Throws_divide_by_zero()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 2; i++)
            {

                var task = Task.Factory.StartNew(ThreadUnsafe.Go);
                tasks.Add(task);
            }

            try
            {
                foreach (var task in tasks)
                {
                    task.Wait();
                }

                Assert.Fail("No devide by zero");
            }
            catch (Exception e)
            {
                Assert.That(e.InnerException , Is.InstanceOf<DivideByZeroException>());
            }
        }

        [Test]
        public void No_throws_divide_by_zero()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 2; i++)
            {

                var task = Task.Factory.StartNew(ThreadSafe.Go);
                tasks.Add(task);
            }

            try
            {
                foreach (var task in tasks)
                {
                    task.Wait();
                }

            }
            catch (Exception e)
            {
                Assert.Fail("No exception should be thrown");
            }
        }

        [Test]
        public void No_throws_divide_by_zero_monitor()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 2; i++)
            {

                var task = Task.Factory.StartNew(ThreadSafeMonitor.Go);
                tasks.Add(task);
            }

            try
            {
                foreach (var task in tasks)
                {
                    task.Wait();
                }

            }
            catch (Exception e)
            {
                Assert.Fail("No exception should be thrown");
            }
        }

        [Test]
        public void DeadLockTest()
        {
            var thread = new Thread(DeadLock.Go);
            var thread1 = new Thread(DeadLock.Go2);
            thread.Start();
            thread1.Start();

            Thread.Sleep(10000);

            //we seem to be locked
            Assert.That(thread.ThreadState, Is.EqualTo(ThreadState.WaitSleepJoin ));
            Assert.That(thread1.ThreadState, Is.EqualTo(ThreadState.WaitSleepJoin));
            
            thread.Abort();
            thread1.Abort();


        }

        [Test]
        public void Mutex_in_other_process()
        {
            for (int i = 0; i < 10; i++)
            {
                var p = new Process {StartInfo = {WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, FileName = "JustOne.exe"}};

                p.Start();
            }

            Thread.Sleep(10000); // wait a bit for all the process to close

            var processes = Process.GetProcessesByName("JustOne");
            try
            {
                Assert.That(processes.Count(), Is.EqualTo(1));
            }
            finally
            {
                try
                {
                    foreach (var process in processes)
                    {
                        process.Kill();
                    }
                }catch{}

            }
        }

	    [Test]
	    public void Open_a_club()
	    {
		    for (int i = 1; i <= 5; i++)
		    {
			    new Thread(TheEnVy.Enter).Start(i);
		    }
	    }
    }

	public class TheEnVy
	{
		static SemaphoreSlim semaphore = new SemaphoreSlim(3);

		public static void Enter(object id)
		{
			Console.WriteLine(id + " wants to enter");
			semaphore.Wait(); //wait is the gateway if count is less then three you're in
			Console.WriteLine(id + " is in!");
			Thread.Sleep(1000 * (int) id);
			Console.WriteLine(id + " is leaving");
			semaphore.Release();
		}
	}

    public class DeadLock
    {
        static object locker1 = new object();
        static object locker2 = new object();

        public static void Go()
        {
            lock (locker1)
            {
                Thread.Sleep(1000);
                lock (locker2)
                {
                    
                }
            }
        }

        public static void Go2()
        {
            lock (locker2)
            {
                Thread.Sleep(1000);
                lock (locker1)
                {
                    
                }
            }
        }
    }


}