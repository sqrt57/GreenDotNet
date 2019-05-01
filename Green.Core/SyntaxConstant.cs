namespace Green
{
    public sealed class SyntaxConstant : ISyntax
    {
        public SyntaxConstant(
            SyntaxInfo syntaxInfo,
            object value)
        {
            SyntaxInfo = syntaxInfo;
            Value = value;
        }

        public SyntaxInfo SyntaxInfo { get; }
        public object Value { get; }
    }
}
