namespace tutorials.ThreadTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

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
    }
}