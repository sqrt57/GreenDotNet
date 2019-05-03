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
            var result = _reader.Read("").ToArray();
            Assert.Empty(result);
        }

        [Fact]
        public void Read_Number()
        {
            var result = _reader.Read("5").ToArray();
            Assert.Single(result);
            Assert.Equal(SourceType.String, result[0].SyntaxInfo.Source.Type);
            Assert.Null(result[0].SyntaxInfo.Source.FileName);
            Assert.Equal(new SourcePosition(0, 0, 0), result[0].SyntaxInfo.Position);
            Assert.Equal(1, result[0].SyntaxInfo.Span);
            Assert.IsType<SyntaxConstant>(result[0]);
            Assert.Equal(5L, ((SyntaxConstant)result[0]).Value);
        }

        [Fact]
        public void Read_Symbol()
        {
            var result = _reader.Read("x").ToArray();
            Assert.Single(result);
            Assert.Equal(SourceType.String, result[0].SyntaxInfo.Source.Type);
            Assert.Null(result[0].SyntaxInfo.Source.FileName);
            Assert.Equal(new SourcePosition(0, 0, 0), result[0].SyntaxInfo.Position);
            Assert.Equal(1, result[0].SyntaxInfo.Span);
            Assert.IsType<SyntaxIdentifier>(result[0]);
            Assert.Equal("x", ((SyntaxIdentifier)result[0]).Name);
        }

        [Fact]
        public void Read_EmptyList()
        {
            var result = _reader.Read("()").ToArray();
            Assert.Single(result);
            Assert.IsType<SyntaxList>(result[0]);
            Assert.Empty(((SyntaxList)result[0]).Items);
            Assert.Equal(new SourcePosition(0, 0, 0), result[0].SyntaxInfo.Position);
            Assert.Equal(2, result[0].SyntaxInfo.Span);
        }

        [Fact]
        public void Read_List()
        {
            var result = _reader.Read("(+ 2 3)").ToArray();
            Assert.Single(result);
            Assert.IsType<SyntaxList>(result[0]);
            Assert.Equal(new SourcePosition(0, 0, 0), result[0].SyntaxInfo.Position);
            Assert.Equal(7, result[0].SyntaxInfo.Span);
            var subList = (SyntaxList)result[0];
            Assert.Equal(3, subList.Items.Count);
            Assert.IsType<SyntaxIdentifier>(subList.Items[0]);
            Assert.Equal(IdentifierType.Identifier, ((SyntaxIdentifier)subList.Items[0]).Type);
            Assert.Equal("+", ((SyntaxIdentifier)subList.Items[0]).Name);
            Assert.IsType<SyntaxConstant>(subList.Items[1]);
            Assert.Equal(2L, ((SyntaxConstant)subList.Items[1]).Value);
            Assert.IsType<SyntaxConstant>(subList.Items[2]);
            Assert.Equal(3L, ((SyntaxConstant)subList.Items[2]).Value);
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
            Assert.Equal(new SourcePosition(0, 0, 0), result[0].syntaxInfo.Position);
            Assert.Equal(2, result[0].syntaxInfo.Span);
        }

        [Fact]
        public void Scan_Identifier()
        {
            var result = _reader.Scan("abc").ToArray();
            Assert.Single(result);
            Assert.Equal("abc", result[0].lexeme);
            Assert.Equal(new SourcePosition(0, 0, 0), result[0].syntaxInfo.Position);
            Assert.Equal(3, result[0].syntaxInfo.Span);
        }

        [Fact]
        public void Scan_List()
        {
            var result = _reader.Scan("(+ 2\n30)").ToArray();
            Assert.Equal(5, result.Length);

            Assert.Equal("(", result[0].lexeme);
            Assert.Equal(new SourcePosition(0, 0, 0), result[0].syntaxInfo.Position);
            Assert.Equal(1, result[0].syntaxInfo.Span);

            Assert.Equal("+", result[1].lexeme);
            Assert.Equal(new SourcePosition(1, 0, 1), result[1].syntaxInfo.Position);
            Assert.Equal(1, result[1].syntaxInfo.Span);

            Assert.Equal("2", result[2].lexeme);
            Assert.Equal(new SourcePosition(3, 0, 3), result[2].syntaxInfo.Position);
            Assert.Equal(1, result[2].syntaxInfo.Span);

            Assert.Equal("30", result[3].lexeme);
            Assert.Equal(new SourcePosition(5, 1, 0), result[3].syntaxInfo.Position);
            Assert.Equal(2, result[3].syntaxInfo.Span);

            Assert.Equal(")", result[4].lexeme);
            Assert.Equal(new SourcePosition(7, 1, 2), result[4].syntaxInfo.Position);
            Assert.Equal(1, result[4].syntaxInfo.Span);
        }

        [Fact]
        public void EvalToken_Number()
        {
            var (type, value, name) = _reader.EvalToken("12");
            Assert.Equal(TokenType.Number, type);
            Assert.Equal(12L, value);
        }

        [Fact]
        public void EvalToken_NegativeNumber()
        {
            var (type, value, name) = _reader.EvalToken("-12");
            Assert.Equal(TokenType.Number, type);
            Assert.Equal(-12L, value);
        }

        [Fact]
        public void EvalToken_Identifier()
        {
            var (type, value, name) = _reader.EvalToken("abc");
            Assert.Equal(TokenType.Identifier, type);
            Assert.Equal("abc", name);
        }

        [Fact]
        public void EvalToken_Plus_Identifier()
        {
            var (type, value, name) = _reader.EvalToken("+");
            Assert.Equal(TokenType.Identifier, type);
            Assert.Equal("+", name);
        }

        [Fact]
        public void EvalToken_LeftBracket()
        {
            var (type, value, name) = _reader.EvalToken("(");
            Assert.Equal(TokenType.LeftBracket, type);
        }

        [Fact]
        public void EvalToken_RightBracket()
        {
            var (type, value, name) = _reader.EvalToken(")");
            Assert.Equal(TokenType.RightBracket, type);
        }

        [Fact]
        public void ReadInteractive_Symbol_SyntaxInfo()
        {
            var result = _reader.ReadInteractive(new[] { "x" });
            Assert.True(result.Finished);
            Assert.Single(result.Objects);
            Assert.Equal(SourceType.String, result.Objects[0].SyntaxInfo.Source.Type);
            Assert.Null(result.Objects[0].SyntaxInfo.Source.FileName);
            Assert.Equal(new SourcePosition(0, 0, 0), result.Objects[0].SyntaxInfo.Position);
            Assert.Equal(1, result.Objects[0].SyntaxInfo.Span);
        }

        [Fact]
        public void ReadInteractive_Symbol()
        {
            var result = _reader.ReadInteractive(new[] { "x" });
            Assert.True(result.Finished);
            Assert.Single(result.Objects);
            Assert.IsType<SyntaxIdentifier>(result.Objects[0]);
            Assert.Equal("x", ((SyntaxIdentifier)result.Objects[0]).Name);
        }

        [Fact]
        public void ReadInteractive_TwoSymbols()
        {
            var result = _reader.ReadInteractive(new[] { "x y" });
            Assert.True(result.Finished);
            Assert.Equal(2, result.Objects.Count);
            Assert.IsType<SyntaxIdentifier>(result.Objects[0]);
            Assert.Equal("x", ((SyntaxIdentifier)result.Objects[0]).Name);
            Assert.IsType<SyntaxIdentifier>(result.Objects[1]);
            Assert.Equal("y", ((SyntaxIdentifier)result.Objects[1]).Name);
        }
    }
}
