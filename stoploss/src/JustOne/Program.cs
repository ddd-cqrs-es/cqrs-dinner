using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JustOne
{
    using System.Threading;

    class Program
    {
        // {71C4F3A5-DF4D-4F70-B388-B6B0CE5C6CA3}
        static  Guid mutexKey = new Guid("2D5F74B8-485A-434B-9C9F-5866577BDBBB"); 

        static void Main(string[] args)
        {
            using (var mx = new Mutex(false, mutexKey.ToString()))
            {
                if(!mx.WaitOne(3000, false))
                {
                    Console.WriteLine("Another instance is running. bye!");
                    return;
                }
                RunProgram();
            }
        }

        private static void RunProgram()
        {
            Console.WriteLine("Stay a while stay forever");
            while (true)
            {
                
            }
        }
    }
}
