using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NamedPipeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var mode = args[0];

            if (mode == "noread")
            {

                using (var s = new NamedPipeClientStream("x-pipedream"))
                {
                    s.Connect();
                    s.ReadMode = PipeTransmissionMode.Message;
                    Thread.Sleep(1000000);
                }
            }else if (mode == "read")
            {
                using (var s = new NamedPipeClientStream("x-pipedream"))
                {
                    s.Connect();
                    s.ReadMode = PipeTransmissionMode.Message;

                    while (true)
                    {
                        var message = Encoding.UTF8.GetString(ReadMsg(s));

                        Console.WriteLine(message);
                    }
                }
            }else if (mode == "mmf")
            {
                using (var mmf = MemoryMappedFile.OpenExisting("shared-memory"))
                using (var accessor = mmf.CreateViewAccessor())
                {
                    var i = accessor.ReadInt32(0);

                    Console.WriteLine(i);
                    Thread.Sleep(100000);
                }
            }



        }


        private static byte[] ReadMsg(PipeStream s)
        {
            var ms = new MemoryStream();
            var buffer = new byte[0x1000];
            do
            {
                ms.Write(buffer, 0, s.Read(buffer, 0, buffer.Length));
            } while (!s.IsMessageComplete);
            return ms.ToArray();
        }
    }
}
