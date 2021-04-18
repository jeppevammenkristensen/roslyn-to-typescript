namespace RoslynToTypescript
{
    public record Block(string HardCoded) : ISyntax
    {
        public string Display(int i = 0)
        {
            return HardCoded;
        }
    }
}