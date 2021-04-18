using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace RoslynToTypescript
{
    public interface IToken : ISyntax
    {

    }

    public record QuestionToken : IToken
    {
        public string Display(int indentation = 0)
        {
            return "?";
        }
    }

    public record PropertyDeclaration(string Name,  ITypeSyntax? Type, QuestionToken? question = null) : IMemberDeclaration
    {
        public string Display(int i = 0)
        {
            var builder = new StringBuilder();
            builder.AppendWithIndentation(Name, i);
            if (question != null)
            {
                builder.Append(question.Display());
            }

            if (Type != null)
            {
                builder.Append($" : {Type.Display()}");
            }

            return builder.ToString();
        }
    }

    public record TypeReference(string Name, ImmutableArray<TypeParameter>? Parameters) : ITypeSyntax
    {
        public string Display(int i = 0)
        {
            var builder = new StringBuilder();
            builder.Append(Name);

            if (Parameters is not null && Parameters.Value.Length > 0)
            {
                builder.Append($"<{string.Join(",", Parameters.Value.Select(x => x.Display()))}>");
            }

            return builder.ToString();
        }
    }
}