using System;
using System.Collections.Generic;
using System.Linq;

namespace Green
{
    public sealed class Interpreter
    {
        private readonly Reader _reader;

        public Interpreter()
        {
            _reader = new Reader();
        }

        public object EvalSource(string source)
        {
            object result = null;
            foreach (var expr in _reader.Read(source))
                result = Eval(expr);
            return result;
        }

        private object Eval(object expr)
        {
            switch (expr)
            {
                case Int64 intValue:
                    return intValue;
                case IEnumerable<object> list:
                    var fun = list.FirstOrDefault();
                    if (fun == null)
                        throw new SyntaxException("eval: empty application: ()");
                    var evalFun = GetFunction(fun);
                    var args = list.Skip(1).Select(Eval).ToArray();
                    return evalFun(args);
                default:
                    throw new RuntimeException($"eval: cannot evaluate {expr}");
            }
        }

        private GreenFunction GetFunction(object fun)
        {
            if (fun is Identifier id)
            {
                switch (id.Name)
                {
                    case "+": return Add;
                    default:
                        throw new RuntimeException($"Unknow function {id.Name}");
                }
            }
            else
                throw new RuntimeException($"Cannot use {fun} as a function");
        }

        public delegate object GreenFunction(object[] args);

        public object Add(object[] args)
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
                        result += (Int64) intValue;
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
