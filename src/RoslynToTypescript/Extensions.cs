using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;

[assembly: InternalsVisibleTo("RoslynToTypescript.Tests")]
namespace RoslynToTypescript
{
   
    internal static class Extensions
    {
        public static bool Is(this SpecialType source, params SpecialType[] value)
        {
            return value.Any(x => x == source);
        }

        public static ImmutableArray<TSyntax> AsImmutableArray<TSyntax>(this TSyntax syntax) where TSyntax : ISyntax
        {
            return ImmutableArray.Create<TSyntax>(syntax);
        }

        public static string FirstToLower(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;

            return new string(new[] { char.ToLower(source[0]) }.Concat(source.Skip(1)).ToArray());
        }

        public static void AppendLineWithIndentation(this StringBuilder builder, string text, int indentation)
        {
            builder.AppendLine(WithIndentation(text, indentation));
        }

        public static void AppendWithIndentation(this StringBuilder builder, string text, int indentation)
        {
            builder.Append(WithIndentation(text, indentation));
        }

        public static string WithIndentation(this string source, int indentation)
        {
            return $"{Indentation(indentation)}{source}";
        }

        public static string Indentation(int indentation)
        {
            return new string(' ', indentation);
        }

        public static void WriteTypeParameters(this StringBuilder builder, ImmutableArray<TypeParameter>? typeParameters)
        {
            if (!typeParameters.HasValue || typeParameters.Value.Length == 0) return;

            builder.Append("<");

            builder.Append(string.Join(",", typeParameters.Value.Select(x => x.Display())));

            builder.Append(">");
        }
    }
}