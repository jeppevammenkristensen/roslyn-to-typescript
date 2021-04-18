using System;

namespace RoslynToTypescript
{
    public record PredefinedType (TypescriptBuiltInTypes Type) : ITypeSyntax
    {
        public string Display(int i = 0)
        {
            switch (Type)
            {
                case TypescriptBuiltInTypes.String:
                    return "string";
                case TypescriptBuiltInTypes.Number:
                    return "number";
                case TypescriptBuiltInTypes.Boolean:
                    return "boolean";
                case TypescriptBuiltInTypes.Any:
                    return "any";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}