namespace RoslynToTypescript
{
    public record StringLiteral(string value) : IExpression
    {
        public string Display(int i = 0)
        {
            return $"'{value}'";
        }
    }
}