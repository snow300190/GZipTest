using GZipTest.Enums;
using GZipTest.Interfaces;

namespace GZipTest.Models
{
    public class Part : IPart
    {
        public Part(int order, Operations operaton, byte[] originalData)
        {
            Order = order;
            Operaton = operaton;
            OriginalData = originalData;
        }

        public int Order { get; }

        public Operations Operaton { get; }

        public byte[] OriginalData { get; }

        public byte[] ResultData { get; set; }

        public int Count { get; set; }
    }
}
