using System.Collections.Immutable;
using Xunit;
using Xunit.Abstractions;

namespace RoslynToTypescript.Tests
{
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
}