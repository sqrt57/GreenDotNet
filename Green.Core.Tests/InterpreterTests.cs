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
        public void Scan_Add()
        {
            var result = _interpreter.Scan("(+ 2 3)");
            Assert.Equal(new[] { "(", "+", "2", "3", ")" }, result);
        }

        [Fact]
        public void EvalToken_Number()
        {
            var result = _interpreter.EvalToken("12");
            Assert.Equal(12L, result);
        }

        [Fact]
        public void EvalToken_NegativeNumber()
        {
            var result = _interpreter.EvalToken("-12");
            Assert.Equal(-12L, result);
        }
    }
}
