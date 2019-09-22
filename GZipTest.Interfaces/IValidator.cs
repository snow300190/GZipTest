namespace GZipTest.Interfaces
{
    public interface IValidator<TIn,TOut>
    {
        TOut Validate(TIn smth);
    }
}
