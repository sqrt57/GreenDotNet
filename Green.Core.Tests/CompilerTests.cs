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

        [Fact]
        public void Compile_Variable()
        {
            var compiler = new Compiler();
            var expr = new SyntaxIdentifier(null, IdentifierType.Identifier, "a");
            var result = compiler.Compile(expr);

            Assert.Equal(new byte[] { (byte)OpCode.Var1, 0, }, result.Code);
            Assert.Equal(new object[] { "a" }, result.Variables);
        }

        [Fact]
        public void Compile_Call()
        {
            var compiler = new Compiler();
            var expr = new SyntaxList(
                null,
                new ISyntax[]
                {
                    new SyntaxIdentifier(null, IdentifierType.Identifier, "+"),
                    new SyntaxConstant(null, 1),
                    new SyntaxConstant(null, 2),
                });
            var result = compiler.Compile(expr);

            Assert.Equal(
                new byte[] {
                    (byte)OpCode.Var1, 0,
                    (byte)OpCode.Const1, 0,
                    (byte)OpCode.Const1, 1,
                    (byte)OpCode.Call1, 2,
                },
                result.Code);
            Assert.Equal(new object[] { 1, 2 }, result.Constants);
            Assert.Equal(new object[] { "+" }, result.Variables);
        }
    }
}
