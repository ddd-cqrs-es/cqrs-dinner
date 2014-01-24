using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace tutorials.Streams
{
    [TestFixture]
    public class PipeStreams
    {
        [Test]
         public void Read_bytes_from_server()
         {
             new Thread(Server).Start();

             using (var s = new NamedPipeClientStream("pipedream"))
             {
                 s.Connect();
                 var read = 0;
                 while (read <100)
                 {
                     read = s.ReadByte();
                     Console.WriteLine(read + " ");
                 }
             }
         }

        private void Server()
        {
            using (var s = new NamedPipeServerStream("pipedream"))
            {
                s.WaitForConnection();
                byte b = 0x1;
                while (true)
                {
                    s.WriteByte(b++);
                    Thread.Sleep(10);
                }
            }
        }

        [Test]
        public void Read_messages_from_pipe_server()
        {
            new Thread(MessageServer).Start();

            using (var s = new NamedPipeClientStream("pipedream"))
            {
                s.Connect();
                s.ReadMode = PipeTransmissionMode.Message;

                var msg = ReadMsg(s);
                var stringMessage = Encoding.UTF8.GetString(msg);

                Console.WriteLine(stringMessage);
            }
        }

        private byte[] ReadMsg(PipeStream s)
        {
            var ms = new MemoryStream();
            var buffer = new byte[0x1000];
            do
            {
                ms.Write(buffer, 0, s.Read(buffer, 0, buffer.Length));
            } while (!s.IsMessageComplete);
            return ms.ToArray();
        }

        private void MessageServer()
        {
            using (var s = new NamedPipeServerStream("pipedream", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            {
                s.WaitForConnection();
                var msg = Encoding.UTF8.GetBytes("Hello");
               
                    s.Write(msg, 0, msg.Length);
                
            }
        }
    }
}