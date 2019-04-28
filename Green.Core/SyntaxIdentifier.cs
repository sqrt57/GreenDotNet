namespace Green
{
    public sealed class SyntaxIdentifier : ISyntax
    {
        public SyntaxIdentifier(
            SyntaxInfo syntaxInfo,
            IdentifierType type,
            string name)
        {
            SyntaxInfo = syntaxInfo;
            Type = type;
            Name = name;
        }

        public SyntaxInfo SyntaxInfo { get; }
        public IdentifierType Type { get; }
        public string Name { get; }
    }
}
