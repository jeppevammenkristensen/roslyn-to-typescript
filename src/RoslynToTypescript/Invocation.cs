using System.Collections.Immutable;
using System.Linq;

namespace RoslynToTypescript
{
    public record Invocation(IExpression Expression, ImmutableArray<Argument> Arguments) : IExpression
    {
        public string Display(int i = 0)
        {
            return $"{Expression.Display()}({string.Join(",", Arguments.Select(x => x.Display()))})";
        }
    }
}