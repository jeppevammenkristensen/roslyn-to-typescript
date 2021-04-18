using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RoslynToTypescript.Tests.Extensions;
using Xunit.Sdk;

namespace RoslynToTypescript.Tests
{
    public class FileLoadAttribute : DataAttribute
    {
        public string[] FilePath { get; }

        public FileLoadAttribute(params string[] filePath)
        {
            FilePath = filePath;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) throw new ArgumentNullException(nameof(testMethod));
            yield return new object[] {FileUtil.LoadEmbeddedFile(FilePath)};
        }
    }
}