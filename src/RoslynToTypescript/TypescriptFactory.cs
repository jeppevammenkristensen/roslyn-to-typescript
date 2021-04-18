using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace RoslynToTypescript
{
    public record SourceFile(ImmutableArray<IStatement> Statements) : ISyntax
    {
        public string Display(int indentation = 0)
        {
            var builder = new StringBuilder();
            var last = Statements.LastOrDefault();

            foreach (var statement in Statements)
            {
                builder.Append(statement.Display(indentation));
                if (last != statement)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }

    public static class TypescriptFactory
    {
        public static InterfaceDeclaration Interface(string name, bool exportable)
        {
            return new InterfaceDeclaration(exportable ? new ExportKeyword() : null, new Identifier(name), null,
                ImmutableArray<IMemberDeclaration>.Empty);
        }
    }
}