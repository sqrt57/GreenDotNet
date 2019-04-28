namespace Green
{
    public sealed class Compiler
    {
        public Bytecode Compile(ISyntax expr)
        {
            var result = new BytecodeBuilder();
            switch (expr)
            {
                case SyntaxConstant constant:
                    var index = result.AddConstant(constant.Value);
                    result.AddCode(OpCode.Const1);
                    result.AddCode((byte)index);
                    break;
                default:
                    throw new CompileException($"compile: cannot compile {expr}");
            }

            return result.ToBytecode();
        }
    }
}
