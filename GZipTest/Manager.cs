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
        private int _writePosition = 0;
        private object rLockObj;
        private object wLockObj;
        private FileStream _rFileStream;
        private FileStream _wFileStream;
        private MemoryStream _readBufferStream;

        public Manager(string sourceFilePath, string destinationFilePath, Operations operaton)
        {
            SourceFilePath = sourceFilePath;
            DestinationFilePath = destinationFilePath;
            Operaton = operaton;
            AvailableThreadsCount = Environment.ProcessorCount;
            AvailebleMemory = 1 * 1024 * 1024 * 1024;
            AvailebleMemoryPerThread = AvailebleMemory / AvailableThreadsCount / 20;

            _waitingParts = new ConcurrentDictionary<int, IPart>();

            rLockObj = new object();
            wLockObj = new object();

            if (Operaton == Operations.Compress && string.IsNullOrWhiteSpace(Path.GetExtension(destinationFilePath)))
                destinationFilePath += ".gz";

            _rFileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            _wFileStream = new FileStream(destinationFilePath, FileMode.CreateNew, FileAccess.Write);

            _readBufferStream = new MemoryStream();
        }

        public string SourceFilePath { get; set; }
        public string DestinationFilePath { get; set; }
        public Operations Operaton { get ; set; }

        public int AvailableThreadsCount { get; }

        public int AvailebleMemory { get; }

        public int AvailebleMemoryPerThread { get; }
        public Progress CurrentProgress { get; set; }

        public IPart ReadNextPartOfFile()
        {
            lock (rLockObj)
            {
                if (CurrentProgress == Progress.InProcess)
                {
                    _rFileStream.CopyTo(_readBufferStream, AvailebleMemoryPerThread);
                    if (_readBufferStream.Length > 0)
                    {
                        Part part = new Part(_currentPart, Operaton, _readBufferStream.ToArray());
                        _currentPart++;
                        _readBufferStream.SetLength(0);
                        return part;
                    }
                    CurrentProgress = Progress.Done;
                    _rFileStream.Dispose();
                    _readBufferStream.Dispose();
                }
                return null;
            }
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
                        _wFileStream.Write(part.ResultData, 0, part.ResultData.Length);
                        _wFileStream.Flush();
                        _waitingParts.TryRemove(part.Order, out IPart removedPart);
                        _processedParts++;
                    }
                }
                try
                {
                    Program.PBar.Report(_rFileStream.Position / (double)_rFileStream.Length);
                }
                catch { }
            }
            if (CurrentProgress == Progress.Done && _waitingParts.Count == 0)
            {
                _wFileStream.Unlock(0, _wFileStream.Length-1);
                _wFileStream.Dispose();
            }
        }
    }
}
