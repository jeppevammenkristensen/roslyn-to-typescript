using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace RoslynToTypescript
{
    /// <summary>
    /// F.eks () => { }  (name:string) => {}
    /// </summary>
    public record ArrowFunction (ImmutableArray<Parameter> Parameters, Block Block) : IExpression
    {
        public string Display(int i = 0)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"({string.Join(", ", Parameters.Select(x => x.Display()))}) => {{}}");
            return builder.ToString();
        }
    }
}