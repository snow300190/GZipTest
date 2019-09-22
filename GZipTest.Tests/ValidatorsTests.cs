using GZipTest.Constants;
using GZipTest.Interfaces;
using GZipTest.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace GZipTest.Tests
{
    [TestClass]
    public class ValidatorsTests
    {
        private IValidator<string, string> _pathValidator;
        private string _currentDir;
        private string _fileName;

        public ValidatorsTests()
        {
            _pathValidator = new PathValidator();
            _currentDir = AppDomain.CurrentDomain.BaseDirectory;
            _fileName = "for_test.txt";
        }
        [TestMethod]
        public void PathValidator_IsValid_Test()
        {
            string resultPath = _currentDir + _fileName;
            if (!File.Exists(resultPath))
            {
                var file = File.Create(resultPath);
                file.Dispose();
            }
            string validationResult = _pathValidator.Validate(resultPath);
            Assert.IsNull(validationResult);
            File.Delete(resultPath);
        }
        [TestMethod]
        public void PathValidator_IsNotValid_Test()
        {
            string resultPath = _currentDir + $"{Guid.NewGuid().ToString()}.txt";
            string validationResult = _pathValidator.Validate(resultPath);
            Assert.IsNotNull(validationResult);
            Assert.AreEqual(validationResult, Errors.FILE_IS_NOT_EXISTS);
        }
    }
}
