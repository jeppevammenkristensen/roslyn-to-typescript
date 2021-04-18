namespace RoslynToTypescript
{
    public record Parameter (string Name, ITypeSyntax? Type) : ISyntax
    {
        public string Display(int i = 0)
        {
            string result = Type switch
            {
                null => $"{Name}",
                _ => $"{Name}:{Type.Display()}"
            };

            return result;
        }
    }
}