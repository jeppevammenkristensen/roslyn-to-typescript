namespace RoslynToTypescript
{
    public record SimpleMemberAccess(IExpression Expression, string Name) : IExpression
    {
        public string Display(int i = 0)
        {
            return $"{Expression.Display()}.{Name}";
        }
    }
}