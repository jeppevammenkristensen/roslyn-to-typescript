using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator;
using RoslynToTypescript.Tests.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace RoslynToTypescript.Tests
{
    public class SemanticSerializerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SemanticSerializerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void BuildInterfaces()
        {
            var harness = new TestHarness(FileUtil.LoadEmbeddedFile("Files","BuildInterfaces.cs"));
            var first = harness.Compilation.GlobalNamespace.GetTypeMembers().Where(x => x.DeclaringSyntaxReferences.Length > 0);

            int i = 0;

            //var result = harness.Subject.BuildInterfaces(first);
            //_testOutputHelper.WriteLine(result);
        }

        [Fact]
        public void BuildSignalROnCall_ValidMethod_ReturnsExpected()
        {
            var harness = new TestHarness(@"public class Someclass
{
    public void CallMyMethod(int number, string? otherNumber, boolean booleanValue, string[] values)
    {

    }
}");
            var namedTypeSymbol = harness.Compilation.GlobalNamespace.GetTypeMembers().First();
            var first = namedTypeSymbol.GetMembers().OfType<IMethodSymbol>().First();
            _testOutputHelper.WriteLine(harness.Subject.BuildSignalROnCall(first
                ));
            
        }

        public class TestHarness
        {
            public Compilation Compilation
            {
                get;
            }

            public CompilationUnitSyntax Syntax { get; }

            public SemanticSerializer Subject
            {
                get;
            }

            public TestHarness(string codeText)
            {
                var code = SyntaxFactory.ParseCompilationUnit(codeText);
                Syntax = code;
                var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
                ImmutableList<MetadataReference> references = ImmutableList<MetadataReference>.Empty.Add(Mscorlib);

                foreach (var file in Directory.GetFiles(assemblyPath, "*.dll"))
                {
                    FileInfo info = new FileInfo(file);
                    if (Regex.IsMatch(info.Name, "(System.*)|(Microsoft.*)|netstandard\\.dll") && !info.Name.Contains("amd64"))
                        references = references.Add(MetadataReference.CreateFromFile(file));
                }

                Compilation = CSharpCompilation.Create("MyCompilation",
                    syntaxTrees: new []{ code.SyntaxTree}, references: references, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                Subject = new SemanticSerializer(Compilation);
            }
        }
    }
}
