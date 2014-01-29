using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Threading;
using NUnit.Framework;

namespace tutorials.Streams
{
    [TestFixture]
    public class MemoryMappedFiles
    {
        private AutoResetEvent _gate=new AutoResetEvent(false);

        [Test]
        public void Write_to_mmf()
         {
            new Thread(StartServer).Start();
            _gate.WaitOne();
            using (var process = new Process(){StartInfo = new ProcessStartInfo("NamedPipeClient.exe", "mmf")})
            {
                process.Start();
                Thread.Sleep(TimeSpan.FromSeconds(2));
                process.Kill();
            }

            _gate.Set();

         }

        private void StartServer()
        {
            using (var mmf = MemoryMappedFile.CreateNew("shared-memory", 0x1000))
            using (var accessor = mmf.CreateViewAccessor())
            {
                _gate.Set();
                accessor.Write(0, 12345);
                _gate.WaitOne();
            }
            
        }
    }
}