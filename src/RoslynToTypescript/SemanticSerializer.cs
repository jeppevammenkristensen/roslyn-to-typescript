using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Roslynator.CSharp;
using RoslynToTypescript;
[assembly:InternalsVisibleTo("RoslynToTypescript.Tests")]
namespace RoslynToTypescript
{
    internal class ToTypescriptHelper
    {
        private static readonly HashSet<string> KnownGenericCollectionNames = new HashSet<string>()
        {
            "List", "IList", "ICollection", "IEnumerable", "HashSet", "ImmutableArray", "ImmutableList",
            "ReadOnlyCollection", "ObservableCollection", "ReadOnlyObservableCollection", "ISet"
        };
        
      
        
        /// <summary>
        /// Return the underlying type if it is a collection. Otherwise returns null
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <returns></returns>
        internal static ITypeSymbol? IsSomeKindOfCollection(INamedTypeSymbol namedTypeSymbol)
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
        

        internal static (ITypeSymbol? symbol, bool nullable) IsNullable(ITypeSymbol symbol)
        {
            return symbol switch
            {
                {NullableAnnotation: not NullableAnnotation.Annotated} => (null, false),
                {Name: not "Nullable"} => (symbol, true),
                INamedTypeSymbol named => (named.TypeArguments.Single(), true),
                _ => throw new InvalidOperationException("Is nullable but not named type")
            };
        }
    }

    internal class InterfaceBuildVisitor 
    {
        public Compilation Compilation { get; }
        
       private ImmutableArray<IMemberDeclaration> _members = ImmutableArray<IMemberDeclaration>.Empty;

       public HashSet<INamedTypeSymbol> OtherClasses { get; } = new HashSet<INamedTypeSymbol>();

        public InterfaceBuildVisitor(Compilation compilation, bool fetchOtherClasses)
        {
            Compilation = compilation;
        }

        public InterfaceDeclaration Visit(INamedTypeSymbol symbol)
        {
            return new InterfaceDeclaration(new ExportKeyword(), new Identifier(symbol.Name), null,
                VisitMembers(symbol.GetMembers()));
        }


        private ImmutableArray<IMemberDeclaration> VisitMembers(ImmutableArray<ISymbol> members)
        {
            var result = ImmutableArray<IMemberDeclaration>.Empty;
            
            foreach (var member in members)
            {
                switch (member)
                {
                    case IPropertySymbol propertySymbol:
                        result = result.Add(VisitProperty(propertySymbol));
                        break;
                }
            }

            return result;
        }
    

        public PropertyDeclaration VisitProperty(IPropertySymbol symbol)
        {
            var typeVisitor = new TypeVisitor();
            var result = typeVisitor.Visit(symbol.Type);
            
            foreach (var other in typeVisitor._others)
            {
                this.OtherClasses.Add(other);
            }

            QuestionToken? token = result switch
            {
                NullableType => new QuestionToken(),
                _ => null
            };

            return new PropertyDeclaration(symbol.Name.FirstToLower(), result, token);
        }

    }
    
    
    public class SemanticSerializer
    {
       

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
            var typeVisitor = new TypeVisitor();
            return typeVisitor.Visit(symbol);
        }

        public string BuildInterface(INamedTypeSymbol namedTypeSymbol)
        {
           

            var statements = ImmutableArray<IStatement>.Empty;
            
            var interfaceVisitor = new InterfaceBuildVisitor(this.Compilation, true);
            statements = statements.Add(interfaceVisitor.Visit(namedTypeSymbol));
            if (interfaceVisitor.OtherClasses.Count > 0)
            {
                var otherSymbols = interfaceVisitor.OtherClasses.ToList();
                foreach (var otherSymbol in otherSymbols)
                {
                    statements = statements.Add(interfaceVisitor.Visit(otherSymbol));
                }
            }
            
            var sourceFile = new SourceFile(statements);
            return sourceFile.Display();
        }

        private InterfaceDeclaration BuildSingleInterface(INamedTypeSymbol namedTypeSymbol)
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