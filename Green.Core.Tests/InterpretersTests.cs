using System;
using Xunit;

namespace Green.Tests
{
    public class InterpretersTests
    {
        [Fact]
        public void TestCreate()
        {
            var interpreter = new Interpreter();
            Assert.NotNull(interpreter);
        }
    }
}
