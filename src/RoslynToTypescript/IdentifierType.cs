using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynToTypescript
{
    public record IdentifierType (string Name) : ITypeSyntax
    {
        public string Display(int i = 0)
        {
            return Name;
        }
    }

    /// <summary>
    /// Nullable type is not a definition as such in the AST for typescript
    /// But by defining it here, we can de
    /// </summary>
    public record NullableType (ITypeSyntax Type) : ITypeSyntax
    {
        public string Display(int indentation = 0)
        {
            return Type.Display(indentation);
        }
    }
}