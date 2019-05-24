using System.Collections.Generic;

namespace Green
{
    public sealed class Interpreter
    {
        private readonly Reader _reader;
        private readonly IModule _module;

        public Interpreter()
        {
            _reader = new Reader();
            _module = new ReadonlyModule(
                name: "main",
                globals: new Dictionary<string, object>
                {
                    ["+"] = new Types.GreenFunction(BaseLibrary.Add),
                });
        }

        public object EvalSource(string source)
        {
            object result = null;
            foreach (var expr in _reader.Read(source))
                result = Eval(expr);
            return result;
        }

        public object Eval(ISyntax expr)
        {
            var compiler = new Compiler();
            var bytecode = compiler.Compile(expr);
            return Evaluator.Eval(_module, bytecode);
        }

    }
}
