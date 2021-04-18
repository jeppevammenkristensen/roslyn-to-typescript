using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static RoslynToTypescript.TypescriptFactory;

namespace RoslynToTypescript.Tests
{
    public class SourceFileTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public SourceFileTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        


        [Theory]
        [FileLoad("Files\\SimpleSource.ts")]
        public void SimpleSourceFileWithInterface(string expected)
        {
            var source = new SourceFile(
                    Interface("Test2", true).AsImmutableArray<IStatement>()
                        .Add(Interface("Test3",false))
                       );
            var result = source.Display();
            result.Should().Be(expected);
        }
    }


    public class InterfaceDeclarationTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public InterfaceDeclarationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Display_Test()
        {
            var subject = new InterfaceDeclaration(new ExportKeyword(), new Identifier("JeppesTest"), null,
                ImmutableArray<IMemberDeclaration>.Empty
                    .Add(new PropertyDeclaration("name", new PredefinedType(TypescriptBuiltInTypes.String)))
                    .Add(new PropertyDeclaration("isTrue", new PredefinedType(TypescriptBuiltInTypes.Boolean)))
                    .Add(new PropertyDeclaration("array", new ArrayType(new TypeReference("Super", 
                        ImmutableArray<TypeParameter>.Empty
                            .Add(new TypeParameter(new PredefinedType(TypescriptBuiltInTypes.Boolean))))))
                    )
            );

            
            _testOutputHelper.WriteLine(subject.Display());
        }
    }

    public class FileLoadAttribute : DataAttribute
    {
        public string FilePath { get; }

        public FileLoadAttribute(string filePath)
        {
            FilePath = filePath;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) throw new ArgumentNullException(nameof(testMethod));

            var path = Path.IsPathRooted(FilePath)
                ? FilePath
                : Path.GetRelativePath(Directory.GetCurrentDirectory(), FilePath);

            if (!File.Exists(path))
            {
                throw new ArgumentException("Path was not found");
            }

            yield return new object[] {File.ReadAllText(path).Trim()};
        }
    }
}