using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

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
        [FileLoad("Files","SimpleSource.ts")]
        public void SimpleSourceFileWithInterface(string expected)
        {
            var source = new SourceFile(
                TypescriptFactory.Interface("Test2", true).AsImmutableArray<IStatement>()
                    .Add(TypescriptFactory.Interface("Test3",false))
            );
            var result = source.Display();
            result.Should().Be(expected);
        }
    }
}