using GZipTest.Enums;

namespace GZipTest.Interfaces
{
    public interface IManager
    {
        string SourceFilePath { get; set; }
        string DestinationFilePath { get; set; }
        Operations Operaton { get; set; }
        int AvailableThreadsCount { get; }
        int AvailebleMemory { get; }
        int AvailebleMemoryPerThread { get; }
        Progress CurrentProgress { get; }


        IPart ReadNextPartOfFile();
        bool WritePartOfFile(IPart part);
    }
}
