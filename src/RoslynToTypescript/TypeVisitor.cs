using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp;

namespace RoslynToTypescript
{
    

    public class TypeVisitor
    {
        public ImmutableHashSet<INamedTypeSymbol> _others = ImmutableHashSet<INamedTypeSymbol>.Empty;
        
        public ITypeSyntax Visit(ITypeSymbol symbol, bool skipNullcheck = false)
        {
            if (!skipNullcheck)
            {
                var result = IsNullable(symbol);
                if (result != null)
                    return new NullableType(Visit(result, true));
            }

            switch (symbol)
            {
                case INamedTypeSymbol namedTypeSymbol:
                    return VisitNamedTypeSymbol(namedTypeSymbol);
                case IArrayTypeSymbol arrayTypeSymbol:
                    return VisitArrayTypeSymbol(arrayTypeSymbol);
            }

            return new PredefinedType(TypescriptBuiltInTypes.Any);
        }

        private ITypeSyntax VisitNamedTypeSymbol(INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.DeclaringSyntaxReferences.Length > 0)
            {
                _others = _others.Add(namedTypeSymbol);
            }
            
            if (CSharpFacts.IsNumericType(namedTypeSymbol.SpecialType))
                return new PredefinedType(TypescriptBuiltInTypes.Number);

            // Håndter andre kendte type
            var result = namedTypeSymbol.SpecialType switch 
            {
                SpecialType.System_String => new PredefinedType(TypescriptBuiltInTypes.String),
                SpecialType.System_Char => new PredefinedType(TypescriptBuiltInTypes.String),
                SpecialType.System_DateTime => new PredefinedType(TypescriptBuiltInTypes.String),
                SpecialType.System_Boolean => new PredefinedType(TypescriptBuiltInTypes.Boolean),
                _  => null
            };

            if (result is not null) return result;

            if (namedTypeSymbol.SpecialType.Is(SpecialType.System_Object, SpecialType.System_Enum, SpecialType.None))
            {
                var typeReference = new TypeReference(namedTypeSymbol.Name, null);

                if (namedTypeSymbol.IsGenericType)
                {
                    var match = ToTypescriptHelper.IsSomeKindOfCollection(namedTypeSymbol);
                    if (match is not null)
                    {
                        return new ArrayType(Visit(match));
                    }

                    typeReference = typeReference with
                    {
                        Parameters = ImmutableArray<TypeParameter>.Empty.AddRange(
                            namedTypeSymbol.TypeArguments.Select(x =>
                                new TypeParameter(new IdentifierType(x.Name))))
                    };
                }

                return typeReference;
            }

            return new PredefinedType(TypescriptBuiltInTypes.Any);
        }

        private ITypeSyntax VisitArrayTypeSymbol(IArrayTypeSymbol namedTypeSymbol)
        {
            return new ArrayType(Visit(namedTypeSymbol.ElementType));
        }
        
        internal ITypeSymbol? IsNullable(ITypeSymbol symbol)
        {
            return symbol switch
            {
                {NullableAnnotation: not NullableAnnotation.Annotated} => null,
                {Name: not "Nullable"} => symbol,
                INamedTypeSymbol named => named.TypeArguments.Single(),
                _ => throw new InvalidOperationException("Is nullable but not named type")
            };
        }
    }
}