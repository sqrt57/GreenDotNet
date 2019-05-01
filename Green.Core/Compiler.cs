namespace Green
{
    public sealed class Compiler
    {
        public Bytecode Compile(ISyntax expr)
        {
            var builder = new BytecodeBuilder();
            Compile(builder, expr);
            return builder.ToBytecode();
        }

        private void Compile(BytecodeBuilder builder, ISyntax expr)
        {
            switch (expr)
            {
                case SyntaxConstant constant:
                    var constIndex = builder.AddConstant(constant.Value);
                    builder.AddCode(OpCode.Const1);
                    builder.AddCode((byte)constIndex);
                    break;
                case SyntaxIdentifier identifier:
                    if (identifier.Type != IdentifierType.Identifier)
                        throw new CompileException($"compile: cannot compile keyword {identifier.Name}");
                    var varIndex = builder.AddVariable(identifier.Name);
                    builder.AddCode(OpCode.Var1);
                    builder.AddCode((byte)varIndex);
                    break;
                case SyntaxList list:
                    if (list.Items.Count == 0)
                        throw new CompileException($"compile: cannot compile empty application: ()");
                    foreach (var subExpr in list.Items)
                        Compile(builder, subExpr);
                    builder.AddCode(OpCode.Call1);
                    builder.AddCode((byte)(list.Items.Count - 1));
                    break;
                default:
                    throw new CompileException($"compile: cannot compile {expr}");
            }
        }
    }
}
