namespace RoslynToTypescript
{
    public record Identifier(string Name) : IExpression
    {
        public string Display(int i = 0)
        {
            return Name;
        }
    }
}