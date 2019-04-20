using System;
using Xunit;

namespace Green.Tests
{
    public class InterpreterTests
    {
        private readonly Interpreter _interpreter;

        public InterpreterTests()
        {
            _interpreter = new Interpreter();
        }

        [Fact]
        public void Eval_Add()
        {
            var result = _interpreter.Eval("(+ 2 3)");
            Assert.Equal(5, result);
        }

        [Fact]
        public void Read_Empty()
        {
            var result = _interpreter.Read("");
            Assert.Equal(new object[] { }, result);
        }

        [Fact]
        public void Read_Number()
        {
            var result = _interpreter.Read("5");
            Assert.Equal(new object[] { 5L }, result);
        }

        [Fact]
        public void Read_EmptyList()
        {
            var result = _interpreter.Read("()");
            Assert.Equal(new[] { new object[] { } }, result);
        }

        [Fact]
        public void Read_List()
        {
            var result = _interpreter.Read("(+ 2 3)");
            Assert.Equal(new[] { new object[] { _interpreter.ToIdentifier("+"), 2L, 3L } }, result);
        }

        [Fact]
        public void Scan_Empty()
        {
            var result = _interpreter.Scan("");
            Assert.Equal(new string[] { }, result);
        }

        [Fact]
        public void Scan_Whitespace()
        {
            var result = _interpreter.Scan(" ");
            Assert.Equal(new string[] { }, result);
        }

        [Fact]
        public void Scan_Number()
        {
            var result = _interpreter.Scan("3");
            Assert.Equal(new[] { "3" }, result);
        }

        [Fact]
        public void Scan_Identifier()
        {
            var result = _interpreter.Scan("abc");
            Assert.Equal(new[] { "abc" }, result);
        }

        [Fact]
        public void Scan_List()
        {
            var result = _interpreter.Scan("(+ 2 3)");
            Assert.Equal(new[] { "(", "+", "2", "3", ")" }, result);
        }

        [Fact]
        public void EvalToken_Number()
        {
            var (type, result) = _interpreter.EvalToken("12");
            Assert.Equal(TokenType.Number, type);
            Assert.Equal(12L, result);
        }

        [Fact]
        public void EvalToken_NegativeNumber()
        {
            var (type, result) = _interpreter.EvalToken("-12");
            Assert.Equal(TokenType.Number, type);
            Assert.Equal(-12L, result);
        }

        [Fact]
        public void EvalToken_Identifier()
        {
            var (type, result) = _interpreter.EvalToken("abc");
            Assert.Equal(TokenType.Identifier, type);
            Assert.Equal(_interpreter.ToIdentifier("abc"), result);
        }

        [Fact]
        public void EvalToken_Plus_Identifier()
        {
            var (type, result) = _interpreter.EvalToken("+");
            Assert.Equal(TokenType.Identifier, type);
            Assert.Equal(_interpreter.ToIdentifier("+"), result);
        }

        [Fact]
        public void EvalToken_LeftBracket()
        {
            var (type, _) = _interpreter.EvalToken("(");
            Assert.Equal(TokenType.LeftBracket, type);
        }

        [Fact]
        public void EvalToken_RightBracket()
        {
            var (type, _) = _interpreter.EvalToken(")");
            Assert.Equal(TokenType.RightBracket, type);
        }
    }
}
