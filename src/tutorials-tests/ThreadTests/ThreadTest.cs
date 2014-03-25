namespace tutorials.ThreadTests
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    [TestFixture]
    public class ThreadTestFixture
    {

        private void WriteY()
        {
            for (var i = 0; i < 1000; i++)
            {
                Console.Write("y");
            }
        }

        [Test]
        public void StartSimpleThread()
        {
            var t = new Thread(WriteY);
            t.Start();

            for (int i = 0; i < 1000; i++)
            {
                Console.Write("x");
            }
        }

        [Test]
        public void ShareState()
        {
            var tt = new ThreadTest();
            new Thread(tt.Go).Start();
            tt.Go();

        }

        [Test]
        public void ShareStateWithLock()
        {
            var tt = new ThreadTestLocked();
            new Thread(tt.Go).Start();
            tt.Go();

        }

        [Test]
        public void JoinTest()
        {
            var t = new Thread(GoJoin);
            t.Start();
            t.Join();
            Console.WriteLine("We have been waited forerver for this thread to end :)");
        }

        private void GoJoin()
        {
            for (int i = 0; i < 1000; i++)
            {
                Console.Write("y");
            }
        }

        [Test]
        public void StartWithParameter()
        {
            var t = new Thread(x => Console.WriteLine("Hello " + x));
            t.Start("Joe");

        }

        [Test]
        public void NameThread()
        {
            //Thread.CurrentThread.Name = "main";
            var t = new Thread(GoNamedThread);
            t.Name = "worker";
            t.Start();
            GoNamedThread();
        }

        private void GoNamedThread()
        {
            Console.WriteLine("Hello from " + Thread.CurrentThread.Name);
        }

        [Test]
        public void BackGroungThread()
        {
            var t = new Thread(() =>
            {
                for (int i = 0; i < 100000000; i++)
                {
                    Console.Write("x");
                }
            });
            t.IsBackground = true;

            t.Start();

            Thread.Sleep(10);

        }

        [Test]
        public void EceptionHandling()
        {
            try
            {
                //would crash if nunit runner would not wait and catch
                new Thread(GoThrows).Start();
            }
            catch
            {
                Console.WriteLine("exception!!!");
            }
        }

        private void GoThrows()
        {
            throw null;
        }

        [Test]
        public void ExceptionHandlingInThread()
        {

            var t = new Thread(GoThrowsAndCatch);
            t.Start();
            t.Join();
            
            
            if (caughtException != null) Console.WriteLine("exception!!!");
            
        }

        private void GoThrowsAndCatch()
        {
            try
            {
                throw null;
            }
            catch(Exception exc)
            {
                caughtException = exc;
            }
        }

        private Exception caughtException;



    }

    public class ThreadTest
    {
        private bool done;

        public void Go()
        {
            if (!done)
            {
                Console.WriteLine("Done");
                done = true; 
            }
        }
    }

    public class ThreadTestLocked
    {
        private bool done;
        private readonly object locker = new object();

        public void Go()
        {
            lock (locker)
            {
                if (!done)
                {
                    Console.WriteLine("Done");
                    done = true;
                }
            }
        }


    }

    [TestFixture]
    public class ThreadPoolFixtures
    {
        [Test]
        public void HelloThreadPool()
        {
            Task.Factory.StartNew(GoTP);
        }

        private void GoTP()
        {
            Console.WriteLine("Hello from thread pool!");
        }

        [Test]
        public void TaskResult()
        {
            var t = Task.Factory.StartNew(() => DownloadString("http://www.linqpad.net"));
            RunSomeOtherStuff();

            var r = t.Result;

            Console.WriteLine(r.Substring(0,80) + "...");
        }



        private void RunSomeOtherStuff()
        {
            Console.WriteLine("Doing stuff");
            Thread.Sleep(1000);
            Console.WriteLine("Done doint stuff");
        }

        private string DownloadString(string url)
        {
            using (var wc = new WebClient())
            {
                return wc.DownloadString(url);
            }
        }

        [Test]
        public void TaskExceptionHandling()
        {
            var t = Task.Factory.StartNew(() => { throw new NullReferenceException("foobaz!"); });
            try
            {
                t.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}