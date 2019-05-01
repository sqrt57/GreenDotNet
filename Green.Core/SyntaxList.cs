using System.Collections.Generic;

namespace Green
{
    public sealed class SyntaxList : ISyntax
    {
        public SyntaxList(
            SyntaxInfo syntaxInfo,
            IReadOnlyList<ISyntax> items)
        {
            SyntaxInfo = syntaxInfo;
            Items = items;
        }

        public SyntaxInfo SyntaxInfo { get; }
        public IReadOnlyList<ISyntax> Items { get; }
    }
}
