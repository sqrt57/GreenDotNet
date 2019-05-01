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

        private object Eval(ISyntax expr)
        {
            switch (expr)
            {
                case SyntaxConstant constant:
                    return constant.Value;
                case SyntaxList list:
                    if (list.Items.Count == 0)
                        throw new SyntaxException("eval: empty application: ()");
                    var fun = list.Items[0];
                    var evalFun = GetFunction(fun);
                    var args = list.Items.Skip(1).Select(Eval).ToArray();
                    return evalFun(args);
                case SyntaxIdentifier identifier:
                    throw new NotImplementedException("eval: variables are not implemented");
                default:
                    throw new RuntimeException($"eval: cannot evaluate {expr}");
            }
        }

        private GreenFunction GetFunction(ISyntax fun)
        {
            if (fun is SyntaxIdentifier id)
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
