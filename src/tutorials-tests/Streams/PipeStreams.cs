using System;
using System.Diagnostics;
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

        [Test][Timeout(2000)]
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
            using (var s = new NamedPipeServerStream("x-pipedream", PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            {
                s.WaitForConnection();
                connected = true;
                while (true)
                {
                    var msg = Encoding.UTF8.GetBytes("Hello" + i++);
                    s.Write(msg, 0, msg.Length); //blocks until read?
                }
            }
        }

        int i = 0;
        private bool connected;


        [Test]
        public void Server_blocks_if_no_reads()
        {
            new Thread(MessageServer).Start();

            using (var p = new Process() {StartInfo = new ProcessStartInfo("NamedPipeClient.exe"){Arguments = "noread"}})
            {
                p.Start();

                Thread.Sleep(TimeSpan.FromSeconds(10));

                Assert.That(connected, Is.True);
                Assert.That(i, Is.EqualTo(1));

                p.Kill();
            }
        }

        [Test]
        public void Client_process_reads()
        {
            new Thread(MessageServer).Start();

            using (var p = new Process() { StartInfo = new ProcessStartInfo("NamedPipeClient.exe"){Arguments = "read"} })
            {
                p.Start();

                Thread.Sleep(TimeSpan.FromSeconds(10));

                Assert.That(connected, Is.True);
                Assert.That(i, Is.Not.EqualTo(1));

                p.Kill();
            }
        }



    }
}