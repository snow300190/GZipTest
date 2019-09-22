using GZipTest.Enums;
using GZipTest.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace GZipTest.Tests
{
    [TestClass]
    public class ArchivatorTests
    {
        private IManager _manager;
        private IArchivator _archivator;

        [TestMethod]
        public void Archivator_Compress_Test()
        {
            string testStr = "It's test string";
            string sourceFile = AppDomain.CurrentDomain.BaseDirectory + "test.txt";
            string destinationFile = AppDomain.CurrentDomain.BaseDirectory + "test.gz";
            if (File.Exists(sourceFile))
                File.Delete(sourceFile);
            if (File.Exists(destinationFile))
                File.Delete(destinationFile);
            File.WriteAllText(sourceFile, testStr);
            _manager = new Manager(sourceFile, destinationFile, Operations.Compress);
            _archivator = new Archivator(_manager);
            _archivator.Perform();
            Assert.IsTrue(File.Exists(destinationFile));
            Assert.IsTrue(File.ReadAllBytes(destinationFile).Length > 0);
            File.Delete(sourceFile);
            File.Delete(destinationFile);
        }
        [TestMethod]
        public void Archivator_Decompress_Test()
        {
            string testStr = "It's test string";
            string sourceFile = AppDomain.CurrentDomain.BaseDirectory + "original.txt";
            string destinationFile = AppDomain.CurrentDomain.BaseDirectory + "original.zip";
            string compressedFile = destinationFile;
            string decompressedSourceFile = AppDomain.CurrentDomain.BaseDirectory + "decompress_original.txt";

            if (File.Exists(sourceFile))
                File.Delete(sourceFile);
            if (File.Exists(destinationFile))
                File.Delete(destinationFile);
            if (File.Exists(decompressedSourceFile))
                File.Delete(decompressedSourceFile);

            File.WriteAllText(sourceFile, testStr);

            _manager = new Manager(sourceFile, destinationFile, Operations.Compress);
            _archivator = new Archivator(_manager);
            _archivator.Perform();

            _manager = new Manager(compressedFile, decompressedSourceFile, Operations.Decompress);
            _archivator = new Archivator(_manager);
            _archivator.Perform();

            string decompressedTestStr = File.ReadAllText(decompressedSourceFile);

            Assert.AreEqual(testStr, decompressedTestStr);

            File.Delete(sourceFile);
            File.Delete(destinationFile);
            File.Delete(decompressedSourceFile);
        }
    }
}
