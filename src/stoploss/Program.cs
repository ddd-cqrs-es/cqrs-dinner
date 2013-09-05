namespace stoplosskata
{
    using System;
    using NUnit.Framework;

    public class Program
    {
        public static int Main(string[] args)
        {
            A();
            return 0;
        }

        private static void A()
        {
            Console.WriteLine("A");
            B();
            C();
        }

        private static void C()
        {
            Console.WriteLine("C");
            D();
        }

        private static void D()
        {
            Console.WriteLine("D");
        }

        private static void B()
        {
            Console.WriteLine("B");
        }

    }

    [TestFixture]
    public class ProgramTest
    {
        [Test]
        public void Test_program()
        {
            Assert.That( Program.Main(new string[0]), Is.EqualTo(0));
        }
    }
}