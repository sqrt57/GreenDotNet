using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Green.Tests
{
    public class CompilerTests
    {
        [Fact]
        public void Compile_Constant()
        {
            var compiler = new Compiler();
            var expr = new SyntaxConstant(null, 5L);
            var result = compiler.Compile(expr);

            Assert.Equal(new byte[] { (byte)OpCode.Const1, 0, }, result.Code);
            Assert.Equal(new object[] { 5L }, result.Constants);
        }
    }
}
