using System;
using System.Collections.Generic;
using System.Text;

namespace Green
{
    public sealed class Bytecode
    {
        public IReadOnlyList<byte> Code { get; }
        public IReadOnlyList<object> Constants { get; }
        public IReadOnlyList<string> Variables { get; }

        public Bytecode(
            IReadOnlyList<byte> code,
            IReadOnlyList<object> constants,
            IReadOnlyList<string> variables)
        {
            Code = code;
            Constants = constants;
            Variables = variables;
        }
    }
}
