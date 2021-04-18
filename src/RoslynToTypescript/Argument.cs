namespace RoslynToTypescript
{
    public record Argument(IExpression Expression) : ISyntax
    {
        public string Display(int i = 0)
        {
            return Expression.Display();
        }
    }
}