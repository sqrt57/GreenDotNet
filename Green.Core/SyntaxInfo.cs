using System;

namespace Green
{
    public sealed class SyntaxInfo
    {
        public SyntaxInfo(
            SourceInfo source,
            SourcePosition position,
            int span)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Position = position;
            Span = span;
        }

        public SourceInfo Source { get; }
        public SourcePosition Position { get; }
        public int Span { get; }

        public static SyntaxInfo FromBeginEnd(
            SourceInfo source,
            SourcePosition begin,
            SourcePosition end)
        {
            return new SyntaxInfo(source, begin, end.Position - begin.Position);
        }

        public static SyntaxInfo FromBeginSpan(
            SourceInfo source,
            SourcePosition begin,
            int span)
        {
            return new SyntaxInfo(source, begin, span);
        }

        public static SyntaxInfo FromLeftRight(SyntaxInfo left, SyntaxInfo right)
        {
            return new SyntaxInfo(
                left.Source,
                left.Position,
                right.Position.Position - left.Position.Position + right.Span);
        }
    }
}
