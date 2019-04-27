namespace Green
{
    public sealed class SourcePositionBuilder
    {
        private SourcePosition _current;

        public SourcePosition Current => _current;

        public void Update(char c)
        {
            if (c == '\n')
            {
                _current = new SourcePosition(
                    position: _current.Position + 1,
                    line: _current.Line + 1,
                    column: 0);
            }
            else
            {
                _current = new SourcePosition(
                    position: _current.Position + 1,
                    line: _current.Line,
                    column: _current.Column + 1);
            }

        }
    }
}
