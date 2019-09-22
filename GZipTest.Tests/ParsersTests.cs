using GZipTest.Enums;
using GZipTest.Interfaces;
using GZipTest.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipTest.Tests
{
    [TestClass]
    public class ParsersTests
    {
        private IParser<string, Operations> _operationParser;

        public ParsersTests()
        {
            _operationParser = new OperationParser();
        }

        [TestMethod]
        public void OperationParser_IsCompress_Test()
        {
            string sourceStr = "compress";
            Operations operations = _operationParser.Parse(sourceStr);
            Assert.IsTrue(operations == Operations.Compress);
            sourceStr = "ComPrEsS";
            operations = _operationParser.Parse(sourceStr);
            Assert.IsTrue(operations == Operations.Compress);
        }
        [TestMethod]
        public void OperationParser_IsDecompress_Test()
        {
            string sourceStr = "decompress";
            Operations operations = _operationParser.Parse(sourceStr);
            Assert.IsTrue(operations == Operations.Decompress);
            sourceStr = "dEComPrEsS";
            operations = _operationParser.Parse(sourceStr);
            Assert.IsTrue(operations == Operations.Decompress);
        }
        [TestMethod]
        public void OperationParser_IsNothing_Test()
        {
            string sourceStr = "steqwrtdsfg";
            Operations operations = _operationParser.Parse(sourceStr);
            Assert.IsTrue(operations == Operations.Nothing);
        }
    }
}
