using System;
using System.IO;

namespace RoslynToTypescript.Tests.Extensions
{
    public static class FileLoadExtensions
    {
        public static string TextFromFile(this string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var testPath = Path.IsPathRooted(path) ? path : Path.GetRelativePath(Directory.GetCurrentDirectory(), path);

            if (!File.Exists(testPath)) throw new InvalidOperationException("Could not open path");

            return File.ReadAllText(path);
        }
    }
}