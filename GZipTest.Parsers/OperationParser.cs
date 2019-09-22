using GZipTest.Enums;
using GZipTest.Interfaces;

namespace GZipTest.Parsers
{
    public class OperationParser : IParser<string, Operations>
    {
        public Operations Parse(string smth)
        {
            switch (smth.ToLower())
            {
                case "compress": return Operations.Compress;
                case "decompress": return Operations.Decompress;
                default: return Operations.Nothing;
            }
        }
    }
}
