namespace RoslynToTypescript
{
    public record ArrayType (ITypeSyntax Type) : ITypeSyntax
    {
        public string Display(int i = 0)
        {
            return $"{Type.Display()}[]";
        }
    }
}