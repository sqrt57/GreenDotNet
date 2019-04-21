using Xunit;

namespace Green.Tests
{
    public sealed class ReaderTests
    {
        private readonly Reader _reader = new Reader();

        [Fact]
        public void Read_Empty()
        {
            var result = _reader.Read("");
            Assert.Equal(new object[] { }, result);
        }

        [Fact]
        public void Read_Number()
        {
            var result = _reader.Read("5");
            Assert.Equal(new object[] { 5L }, result);
        }

        [Fact]
        public void Read_EmptyList()
        {
            var result = _reader.Read("()");
            Assert.Equal(new[] { new object[] { } }, result);
        }

        [Fact]
        public void Read_List()
        {
            var result = _reader.Read("(+ 2 3)");
            Assert.Equal(new[] { new object[] { _reader.ToIdentifier("+"), 2L, 3L } }, result);
        }

        [Fact]
        public void Scan_Empty()
        {
            var result = _reader.Scan("");
            Assert.Equal(new string[] { }, result);
        }

        [Fact]
        public void Scan_Whitespace()
        {
            var result = _reader.Scan(" ");
            Assert.Equal(new string[] { }, result);
        }

        [Fact]
        public void Scan_Number()
        {
            var result = _reader.Scan("3");
            Assert.Equal(new[] { "3" }, result);
        }

        [Fact]
        public void Scan_Identifier()
        {
            var result = _reader.Scan("abc");
            Assert.Equal(new[] { "abc" }, result);
        }

        [Fact]
        public void Scan_List()
        {
            var result = _reader.Scan("(+ 2 3)");
            Assert.Equal(new[] { "(", "+", "2", "3", ")" }, result);
        }

        [Fact]
        public void EvalToken_Number()
        {
            var (type, result) = _reader.EvalToken("12");
            Assert.Equal(TokenType.Number, type);
            Assert.Equal(12L, result);
        }

        [Fact]
        public void EvalToken_NegativeNumber()
        {
            var (type, result) = _reader.EvalToken("-12");
            Assert.Equal(TokenType.Number, type);
            Assert.Equal(-12L, result);
        }

        [Fact]
        public void EvalToken_Identifier()
        {
            var (type, result) = _reader.EvalToken("abc");
            Assert.Equal(TokenType.Identifier, type);
            Assert.Equal(_reader.ToIdentifier("abc"), result);
        }

        [Fact]
        public void EvalToken_Plus_Identifier()
        {
            var (type, result) = _reader.EvalToken("+");
            Assert.Equal(TokenType.Identifier, type);
            Assert.Equal(_reader.ToIdentifier("+"), result);
        }

        [Fact]
        public void EvalToken_LeftBracket()
        {
            var (type, _) = _reader.EvalToken("(");
            Assert.Equal(TokenType.LeftBracket, type);
        }

        [Fact]
        public void EvalToken_RightBracket()
        {
            var (type, _) = _reader.EvalToken(")");
            Assert.Equal(TokenType.RightBracket, type);
        }
    }
}
