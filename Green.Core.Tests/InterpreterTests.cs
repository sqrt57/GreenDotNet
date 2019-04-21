using System;
using Xunit;

namespace Green.Tests
{
    public sealed class InterpreterTests
    {
        private readonly Interpreter _interpreter = new Interpreter();

        [Fact]
        public void Eval_Number()
        {
            var result = _interpreter.EvalSource("10");
            Assert.Equal(10L, result);
        }

        [Fact]
        public void Eval_Add()
        {
            var result = _interpreter.EvalSource("(+ 2 3)");
            Assert.Equal(5L, result);
        }

        [Fact]
        public void Eval_NestedAdd()
        {
            var result = _interpreter.EvalSource("(+ (+) (+ 2 3) (+ 4 5 6))");
            Assert.Equal(20L, result);
        }
    }
}
