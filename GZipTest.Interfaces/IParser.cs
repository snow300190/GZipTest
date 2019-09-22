namespace GZipTest.Interfaces
{
    public interface IParser<TIn,TOut>
    {
        TOut Parse(TIn smth);
    }
}
