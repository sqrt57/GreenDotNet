using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Green.Tests
{
    public class EvaluatorTests
    {
        [Fact]
        public void Eval_Constant()
        {
            var module = new ReadonlyModule(
                name: "main",
                globals: new Dictionary<string, object>
                {
                    ["+"] = new Interpreter.GreenFunction(Interpreter.Add),
                });

            var bytecode = new Bytecode(
                code: new byte[] { (byte)OpCode.Const1, 0, },
                constants: new object[] { 3, },
                variables: new string[] { });

            var evaluator = new Evaluator();

            var result = evaluator.Eval(module, bytecode);

            Assert.Equal(3, result);
        }

        [Fact]
        public void Eval_Variable()
        {
            var module = new ReadonlyModule(
                name: "main",
                globals: new Dictionary<string, object>
                {
                    ["x"] = 5,
                });

            var bytecode = new Bytecode(
                code: new byte[] { (byte)OpCode.Var1, 0, },
                constants: new object[] { },
                variables: new string[] { "x", });

            var evaluator = new Evaluator();

            var result = evaluator.Eval(module, bytecode);

            Assert.Equal(5, result);
        }

        [Fact]
        public void Eval_Call()
        {
            var module = new ReadonlyModule(
                name: "main",
                globals: new Dictionary<string, object>
                {
                    ["+"] = new Interpreter.GreenFunction(Interpreter.Add),
                });

            var bytecode = new Bytecode(
                code: new byte[]
                {
                    (byte)OpCode.Const1, 0,
                    (byte)OpCode.Const1, 1,
                    (byte)OpCode.Const1, 2,
                    (byte)OpCode.Call1, 2,
                },
                constants: new object[] { new Interpreter.GreenFunction(Interpreter.Add), 2L, 3L, },
                variables: new string[] { });

            var evaluator = new Evaluator();

            var result = evaluator.Eval(module, bytecode);

            Assert.Equal(5L, result);
        }
    }
}
