using System.Linq;
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
            var result = _reader.Scan("").ToArray();
            Assert.Empty(result);
        }

        [Fact]
        public void Scan_Whitespace()
        {
            var result = _reader.Scan(" ").ToArray();
            Assert.Empty(result);
        }

        [Fact]
        public void Scan_Number()
        {
            var result = _reader.Scan("15").ToArray();
            Assert.Single(result);
            Assert.Equal("15", result[0].lexeme);
            Assert.Equal(SourceType.String, result[0].syntaxInfo.Source.Type);
            Assert.Null(result[0].syntaxInfo.Source.FileName);
            Assert.Equal(0, result[0].syntaxInfo.Position);
            Assert.Equal(0, result[0].syntaxInfo.LineNumber);
            Assert.Equal(0, result[0].syntaxInfo.ColumnNumber);
            Assert.Equal(2, result[0].syntaxInfo.Span);
        }

        [Fact]
        public void Scan_Identifier()
        {
            var result = _reader.Scan("abc").ToArray();
            Assert.Single(result);
            Assert.Equal("abc", result[0].lexeme);
            Assert.Equal(0, result[0].syntaxInfo.Position);
            Assert.Equal(0, result[0].syntaxInfo.LineNumber);
            Assert.Equal(0, result[0].syntaxInfo.ColumnNumber);
            Assert.Equal(3, result[0].syntaxInfo.Span);
        }

        [Fact]
        public void Scan_List()
        {
            var result = _reader.Scan("(+ 2\n30)").ToArray();
            Assert.Equal(5, result.Length);

            Assert.Equal("(", result[0].lexeme);
            Assert.Equal(0, result[0].syntaxInfo.Position);
            Assert.Equal(0, result[0].syntaxInfo.LineNumber);
            Assert.Equal(0, result[0].syntaxInfo.ColumnNumber);
            Assert.Equal(1, result[0].syntaxInfo.Span);

            Assert.Equal("+", result[1].lexeme);
            Assert.Equal(1, result[1].syntaxInfo.Position);
            Assert.Equal(0, result[1].syntaxInfo.LineNumber);
            Assert.Equal(1, result[1].syntaxInfo.ColumnNumber);
            Assert.Equal(1, result[1].syntaxInfo.Span);

            Assert.Equal("2", result[2].lexeme);
            Assert.Equal(3, result[2].syntaxInfo.Position);
            Assert.Equal(0, result[2].syntaxInfo.LineNumber);
            Assert.Equal(3, result[2].syntaxInfo.ColumnNumber);
            Assert.Equal(1, result[2].syntaxInfo.Span);

            Assert.Equal("30", result[3].lexeme);
            Assert.Equal(5, result[3].syntaxInfo.Position);
            Assert.Equal(1, result[3].syntaxInfo.LineNumber);
            Assert.Equal(0, result[3].syntaxInfo.ColumnNumber);
            Assert.Equal(2, result[3].syntaxInfo.Span);

            Assert.Equal(")", result[4].lexeme);
            Assert.Equal(7, result[4].syntaxInfo.Position);
            Assert.Equal(1, result[4].syntaxInfo.LineNumber);
            Assert.Equal(2, result[4].syntaxInfo.ColumnNumber);
            Assert.Equal(1, result[4].syntaxInfo.Span);
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
