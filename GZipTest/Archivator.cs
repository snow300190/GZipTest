using GZipTest.Constants;
using GZipTest.Enums;
using GZipTest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    public class Archivator : IArchivator
    {
        private IManager _manager;

        public Archivator(IManager manager)
        {
            _manager = manager;
        }

        public string Perform()
        {
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < _manager.AvailableThreadsCount; i++) {
                Thread thread = new Thread(new ThreadStart(Zip));
                thread.Start();
                threads.Add(thread);
            }

            foreach(Thread exThread in threads)
            {
                exThread.Join();
            }

            return Responses.SUCCESS;
        }
        void Zip()
        {
            while (_manager.CurrentProgress == Progress.InProcess)
            {
                IPart part = _manager.ReadNextPartOfFile();
                if (part != null)
                {
                    if(part.Operaton == Operations.Compress)
                        Compress(part);
                    else
                        Decompress(part);
                }
            }
        }
        void Compress(IPart part)
        {
            using (Stream sourceStream = new MemoryStream(part.OriginalData))
            {
                using (MemoryStream resultStream = new MemoryStream())
                {
                    using (Stream gZipStream = new GZipStream(resultStream, CompressionMode.Compress))
                    {
                        gZipStream.Write(part.OriginalData, 0, part.Count);
                    }
                    part.ResultData = resultStream.ToArray();
                }
            }
            _manager.WritePartOfFile(part);
        }
        void Decompress(IPart part)
        {
            using (MemoryStream sourceStream = new MemoryStream(part.OriginalData))
            {
                using (MemoryStream resultStream = new MemoryStream())
                {
                    using (Stream gZipStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(resultStream, part.Count);                        
                    }
                    part.ResultData = resultStream.ToArray();
                }
            }
            _manager.WritePartOfFile(part);
        }
    }
}
