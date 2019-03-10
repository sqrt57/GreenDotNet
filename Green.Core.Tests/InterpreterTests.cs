using System;
using Xunit;

namespace Green.Tests
{
    public class InterpreterTests
    {
        [Fact]
        public void TestCreate()
        {
            var interpreter = new Interpreter();
            Assert.NotNull(interpreter);
        }
    }
}
