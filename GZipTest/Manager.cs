using GZipTest.Enums;
using GZipTest.Interfaces;
using GZipTest.Models;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace GZipTest
{
    public class Manager : IManager
    {
        private ConcurrentDictionary<int, IPart> _waitingParts;
        private int _processedParts = 0;
        private int _currentPart = 0;
        private object rLockObj;
        private object wLockObj;
        private FileStream _rFileStream;
        private FileStream _wFileStream;
        private MemoryStream _readBufferStream;
        private ProgressBar PBar;

        public Manager(string sourceFilePath, string destinationFilePath, Operations operaton)
        {
            SourceFilePath = sourceFilePath;
            DestinationFilePath = destinationFilePath;
            Operaton = operaton;
            AvailableThreadsCount =  Environment.ProcessorCount;
            AvailebleMemory = (int)Math.Pow(1024, 3);
            AvailebleMemoryPerThread = AvailebleMemory / AvailableThreadsCount;

            _waitingParts = new ConcurrentDictionary<int, IPart>();

            rLockObj = new object();
            wLockObj = new object();

            if (Operaton == Operations.Compress && string.IsNullOrWhiteSpace(Path.GetExtension(destinationFilePath)))
                destinationFilePath += ".gz";

            _rFileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _wFileStream = new FileStream(destinationFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

            //если файла хватит на меньше чем на заданное количество потоков, то незачем создавать лишние.
            if ((int)_rFileStream.Length / AvailebleMemoryPerThread < AvailableThreadsCount)
                AvailableThreadsCount = ((int)_rFileStream.Length / AvailebleMemoryPerThread) + 1;

            _readBufferStream = new MemoryStream();
            Console.Write(operaton.ToString() + ": ");
            PBar = new ProgressBar();
        }

        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public Operations Operaton { get ; set; }
        public int AvailableThreadsCount { get; }
        public int AvailebleMemory { get; }
        public int AvailebleMemoryPerThread { get; }
        public Progress CurrentProgress { get; set; }
        public void Dispose()
        {
            _rFileStream.Dispose();
            _readBufferStream.Dispose();
            _wFileStream.Dispose();
            PBar.Dispose();
        }

        public IPart ReadNextPartOfFile()
        {
            lock (rLockObj)
            {
                if (CurrentProgress == Progress.InProcess)
                {
                    byte[] buffer = GetBuffer();
                    int readed = _rFileStream.Read(buffer,0,buffer.Length);
                    if (readed > 0)
                    {
                        Part part = new Part(_currentPart, Operaton, buffer);
                        part.Count = readed;
                        _currentPart++;
                        return part;
                    }
                    CurrentProgress = Progress.Done;
                }
                return null;
            }
        }
        private byte[] GetBuffer()
        {
            byte[] resultBuffer;
            if (Operaton == Operations.Decompress)
            {
                byte[] buffer = new byte[4];
                _rFileStream.Read(buffer, 0, buffer.Length);
                int length = BitConverter.ToInt32(buffer, 0);
                resultBuffer = new byte[length];
            }
            else
                resultBuffer = new byte[AvailebleMemoryPerThread];
            return resultBuffer;
        }

        public bool WritePartOfFile(IPart part)
        {
            lock (wLockObj)
            {
                _waitingParts.TryAdd(part.Order, part);
                WriteWaitings();
            }
            return true;
        }
        private void WriteWaitings()
        {
            if (CurrentProgress == Progress.InProcess || _waitingParts.Count > 0)
            {
                int start = _processedParts;
                int count = _waitingParts.Count;
                for (int i = _processedParts; i < count + _processedParts; i++)
                {
                    if (_waitingParts.ContainsKey(i) && i == _processedParts)
                    {
                        IPart part = _waitingParts[i];
                        if(Operaton == Operations.Compress)
                        {
                            byte[] lengthBuffer = BitConverter.GetBytes(part.ResultData.Length);
                            _wFileStream.Write(lengthBuffer, 0, lengthBuffer.Length);
                        }
                        _wFileStream.Write(part.ResultData, 0, part.ResultData.Length);
                        _wFileStream.Flush();
                        _waitingParts.TryRemove(part.Order, out IPart removedPart);
                        _processedParts++;
                    }
                }
                PBar.Report(_rFileStream.Position / (double)_rFileStream.Length);
            }
        }
    }
}
