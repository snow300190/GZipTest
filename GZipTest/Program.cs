using GZipTest.Interfaces;
using System;
using GZipTest.Enums;
using System.Collections.Generic;
using System.Linq;
using GZipTest.Parsers;
using GZipTest.Validators;
using GZipTest.Constants;

namespace GZipTest
{
    public class Program
    {
        private static IParser<string, Operations> _operationParser = new OperationParser();
        private static IValidator<string, string> _pathValidator = new PathValidator();

        static void Main(string[] args)
        {
            List<string> errors = new List<string>();
            Operations operation = args.Length > 0 ? _operationParser.Parse(args[0]) : Operations.Nothing;
            if (operation == Operations.Nothing)
                errors.Add("Nothing to do");

            string inPath = args.Length > 1 ? args[1] : string.Empty;
            string outPath = args.Length > 2 ? args[2] : string.Empty;
            string pathValidationResult = _pathValidator.Validate(inPath);
            if (pathValidationResult != null)
                errors.Add($"{inPath} - {pathValidationResult}");

            if (outPath == null)
                errors.Add($"{outPath} - is empty");

            if (errors.Any())
            {
                Console.WriteLine(string.Join("\\n",errors));
                Console.ReadKey();
            }
            else
            {
                string result;
                IArchivator archivator = null;
                try
                {
                    using (IManager manager = new Manager(inPath, outPath, operation)) {
                        archivator = new Archivator(manager);
                    }
                    result = archivator.Perform();
                }
                catch {
                    result = Responses.NOT_SUCCESS;
                }
                Console.WriteLine(result);
                Console.ReadKey();
                
            }
 
        }
        
    }
}
