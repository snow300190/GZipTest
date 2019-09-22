using GZipTest.Enums;

namespace GZipTest.Interfaces
{
    public interface IPart
    {
        int Order { get; }
        int Count { get; set; }
        Operations Operaton { get; }
        byte[] OriginalData { get; }
        byte[] ResultData { get; set; }
    }
}
