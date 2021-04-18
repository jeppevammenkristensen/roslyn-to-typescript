using System;
using System.IO;

namespace RoslynToTypescript.Tests.Extensions
{
    public static class FileUtil
    {
        public static string LoadEmbeddedFile(params string[] path)
        {
            var assembly = typeof(FileUtil).Assembly;
            string embeddedPath = null;
            try
            {
                embeddedPath = $"RoslynToTypescript.Tests.{string.Join(".", path)}";
                var stream = assembly.GetManifestResourceStream(embeddedPath);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to load manifest resource stream from {embeddedPath}. Available are {Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, assembly.GetManifestResourceNames())}", ex);
            }
        }
    }
}