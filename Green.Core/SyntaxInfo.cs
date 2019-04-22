using System;

namespace Green
{
    public class SyntaxInfo
    {
        public SyntaxInfo(
            SourceInfo source,
            int position,
            int span,
            int lineNumber,
            int columnNumber)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Position = position;
            Span = span;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public SourceInfo Source { get; }
        public int Position { get; }
        public int Span { get; }
        public int LineNumber { get; }
        public int ColumnNumber { get; }
    }
}
