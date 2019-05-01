using System;
using System.Collections.Generic;
using System.Linq;

namespace Green
{
    public sealed class Interpreter
    {
        private readonly Reader _reader;
        private readonly Dictionary<string, object> _environment = new Dictionary<string, object>();
        private Evaluator _evaluator;

        public Interpreter()
        {
            _reader = new Reader();
            _environment.Add("+", new GreenFunction(Add));
            _evaluator = new Evaluator(_environment);
        }

        public object EvalSource(string source)
        {
            object result = null;
            foreach (var expr in _reader.Read(source))
                result = Eval(expr);
            return result;
        }

        private object Eval(ISyntax expr)
        {
            var compiler = new Compiler();
            var bytecode = compiler.Compile(expr);
            return _evaluator.Eval(bytecode);
        }

        public delegate object GreenFunction(object[] args);

        public static object Add(object[] args)
        {
            Int64 result = 0;
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case Int64 intValue:
                        result += intValue;
                        break;
                    case UInt64 intValue:
                        result += (Int64)intValue;
                        break;
                    case Int32 intValue:
                        result += intValue;
                        break;
                    case UInt32 intValue:
                        result += intValue;
                        break;
                    default:
                        throw new RuntimeException($"+: bad argument {arg}");
                }
            }
            return result;
        }
    }
}
