using System;
using System.Collections.Generic;
using System.Text;

namespace Green
{
    public class Evaluator
    {
        private IDictionary<string, object> _environment;

        public Evaluator(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public object Eval(Bytecode bytecode)
        {
            var stack = new Stack<object>();
            int ip = 0;
            while (ip < bytecode.Code.Count)
            {
                var op = (OpCode)bytecode.Code[ip++];
                switch (op)
                {
                    case OpCode.Const1:
                        {
                            var index = bytecode.Code[ip++];
                            var value = bytecode.Constants[index];
                            stack.Push(value);
                            break;
                        }
                    case OpCode.Var1:
                        {
                            var index = bytecode.Code[ip++];
                            var variable = bytecode.Variables[index];
                            var value = _environment[variable];
                            stack.Push(value);
                            break;
                        }
                    case OpCode.Call1:
                        {
                            var argsCount = bytecode.Code[ip++];
                            var args = new object[argsCount];
                            for (int i = argsCount - 1; i >= 0; i--)
                                args[i] = stack.Pop();
                            var fun = (Interpreter.GreenFunction) stack.Pop();
                            stack.Push(fun(args));
                        }
                        break;
                }
            }

            if (stack.Count == 0)
                throw new RuntimeException("eval: No result on stack at the end of evaluation");
            if (stack.Count > 1)
                throw new RuntimeException("eval: Multiple results on stack at the end of evaluation");

            return stack.Pop();
        }
    }
}
