using System.Collections.Immutable;
using System.Text;

namespace RoslynToTypescript
{
    public record InterfaceDeclaration(IModifier? Modifier, Identifier Identifier, ImmutableArray<TypeParameter>? TypeParameters, ImmutableArray<IMemberDeclaration> Members) : IStatement
    {
        public string Display(int indentation = 0)
        {
            var builder = new StringBuilder();

            if (Modifier != null)
            {
                builder.AppendWithIndentation($"{Modifier.Display()} ", indentation);
                builder.Append($"interface {Identifier.Display()}");
            }
            else
            {
                builder.AppendWithIndentation($"interface {Identifier.Display()}", indentation);
            }

            builder.WriteTypeParameters(TypeParameters);

            builder.AppendLine(" {");
            if (Members.Length == 0)
            {
                builder.AppendLine();
            }
            

            foreach (var memberDeclaration in Members)
            {
                builder.AppendLine(memberDeclaration.Display(indentation + 3));
            }

            builder.AppendWithIndentation("}", indentation);

            return builder.ToString();
        }
    }
}