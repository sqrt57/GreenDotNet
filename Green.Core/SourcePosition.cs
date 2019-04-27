using System;
using System.Collections.Generic;
using System.Text;

namespace Green
{
    public struct SourcePosition
    {
        public readonly int Position;
        public readonly int Line;
        public readonly int Column;

        public SourcePosition(int position,
                              int line,
                              int column)
        {
            Position = position;
            Line = line;
            Column = column;
        }

        public override bool Equals(object obj)
        {
            if (obj is SourcePosition other)
                return Equals(other);
            return false;
        }

        public bool Equals(SourcePosition other)
        {
            return Position == other.Position
                && Line == other.Line
                && Column == other.Column;
        }

        public override int GetHashCode()
        {
            return (Position, Line, Column).GetHashCode();
        }
    }
}
