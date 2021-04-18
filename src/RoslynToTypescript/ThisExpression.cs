namespace RoslynToTypescript
{
    public record ThisExpression : IExpression
    {
        public string Display(int i = 0)
        {
            return "this";
        }
    }
}