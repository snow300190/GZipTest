using GZipTest.Interfaces;
using System.IO;
using GZipTest.Constants;

namespace GZipTest.Validators
{
    public class PathValidator : IValidator<string, string>
    {
        public string Validate(string smth)
        {
            if (!File.Exists(smth))
                return Errors.FILE_IS_NOT_EXISTS;
            return null;
        }
    }
}
