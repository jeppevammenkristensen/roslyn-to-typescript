using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using RoslynToTypescript;

namespace RoslynToTypescript
{
    public class SemanticSerializer
    {
        private static readonly HashSet<string> KnownGenericCollectionNames = new HashSet<string>()
        {
            "List", "IList", "ICollection", "IEnumerable", "HashSet", "ImmutableArray", "ImmutableList",
            "ReadOnlyCollection", "ObservableCollection", "ReadOnlyObservableCollection", "ISet"
        };

        public Compilation Compilation { get; }

        public SemanticSerializer(Compilation compilation)
        {
            Compilation = compilation;
        }

        /// <summary>
        /// Build code simular to this._hubconnection.on("Something", (x:string) => {});
        /// </summary>
        /// <param name="methodSymbol"></param>
        /// <returns></returns>
        public string BuildSignalROnCall(IMethodSymbol methodSymbol)
        {
            var simpleMemberAccess = new SimpleMemberAccess(new ThisExpression(), "_hubConnection");
            var invocation = new Invocation(new SimpleMemberAccess(simpleMemberAccess,"on"), ImmutableArray<Argument>.Empty
                .Add(new Argument(new StringLiteral(methodSymbol.Name)))
                .Add(new Argument(new ArrowFunction(GetParameters(methodSymbol.Parameters),
                    new Block(string.Empty))))
            );

            return invocation.Display();

        }

        private ImmutableArray<Parameter> GetParameters(ImmutableArray<IParameterSymbol> methodSymbolParameters)
        {
            var result = ImmutableArray<Parameter>.Empty;

            foreach (var method in methodSymbolParameters)
            {
                result = result.Add(new Parameter(method.Name.FirstToLower(), GetType(method.Type)));
            }

            return result;
        }


        public ITypeSyntax GetType(ITypeSymbol symbol, bool skipNullCheck = false)
        {
            if (!skipNullCheck)
            {
                var (typeSymbol, nullable) = IsNullable(symbol);
                if (nullable)
                {
                    return new NullableType(GetType(typeSymbol!, true));
                }
            }

            if (symbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                return new ArrayType(GetType(arrayTypeSymbol.ElementType));
            }
            
            if (symbol is INamedTypeSymbol namedTypeSymbol)
            {
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

                if (symbol.SpecialType.Is(SpecialType.System_Object, SpecialType.System_Enum, SpecialType.None))
                {
                    var typeReference = new TypeReference(namedTypeSymbol.Name, null);
                    
                    if (namedTypeSymbol.IsGenericType)
                    {
                        var match = IsSomeKindOfCollection(namedTypeSymbol);
                        if (match is not null)
                        {
                            return new ArrayType(GetType(match));
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
            }

            return new PredefinedType(TypescriptBuiltInTypes.Any);
        }

        /// <summary>
        /// Return the underlying type if it is a collection. Otherwise returns null
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns></returns>
        private ITypeSymbol? IsSomeKindOfCollection(INamedTypeSymbol namedTypeSymbol)
        {
            if (!namedTypeSymbol.IsGenericType)
                return null;

            var match = namedTypeSymbol.AllInterfaces.FirstOrDefault(x => x.ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);
            if (match != null)
            {
                return match.TypeArguments.Single();
            }

            if (namedTypeSymbol.TypeKind == TypeKind.Error)
            {
                if (KnownGenericCollectionNames.Contains(namedTypeSymbol.Name))
                {
                    return namedTypeSymbol.TypeArguments.Single();
                }
            }

            return null;
        }
        

        public (ITypeSymbol? symbol, bool nullable) IsNullable(ITypeSymbol symbol)
        {
            return symbol switch
            {
                {NullableAnnotation: not NullableAnnotation.Annotated} => (null, false),
                {Name: not "Nullable"} => (symbol, true),
                INamedTypeSymbol named => (named.TypeArguments.Single(), true),
                _ => throw new InvalidOperationException("Is nullable but not named type")
            };
        }

        public string BuildInterfaces(INamedTypeSymbol namedTypeSymbol)
        {
            var sourceFile = new SourceFile(ImmutableArray<IStatement>.Empty);

            var namedTypeSymbols = Compilation.GlobalNamespace.GetTypeMembers().Where(x => x.DeclaringSyntaxReferences.Length > 0);

            var interf = BuildInterface(namedTypeSymbol);
            var sourcefile = new SourceFile(ImmutableArray.Create<IStatement>().Add(interf));
            return sourcefile.Display();
        }

        public InterfaceDeclaration BuildInterface(INamedTypeSymbol namedTypeSymbol)
        {
            var members = new List<IMemberDeclaration>();
            foreach (var propertySymbol in namedTypeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                members.Add(BuildProperty(propertySymbol));
            }

            return new InterfaceDeclaration(new ExportKeyword(), new Identifier(namedTypeSymbol.Name),
                ImmutableArray<TypeParameter>.Empty, members.ToImmutableArray());
        }

        private PropertyDeclaration BuildProperty(IPropertySymbol symbol)
        {
            var type = GetType(symbol.Type);
            QuestionToken? token = type switch
            {
                NullableType => new QuestionToken(),
                _ => null
            };

            return new PropertyDeclaration(symbol.Name.FirstToLower(), type, token);
        }
    }
}

public interface IStatement : ISyntax
{

}

public interface IModifier : ISyntax
{

}

public record ExportKeyword : IModifier
{
    public string Display(int i = 0)
    {
        return "export";
    }
}

public record TypeParameter(ITypeSyntax Type) : ISyntax
{
    public string Display(int i = 0)
    {
        return Type.Display();
    }
}