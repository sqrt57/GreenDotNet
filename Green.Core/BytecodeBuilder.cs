using System;
using System.Collections.Generic;

namespace Green
{
    public sealed class BytecodeBuilder
    {
        private readonly List<byte> _code = new List<byte>();
        private readonly List<object> _constants = new List<object>();
        private readonly List<string> _variables = new List<string>();

        public void AddCode(OpCode code) => _code.Add((byte) code);

        public void AddCode(byte code) => _code.Add(code);

        public int AddConstant(object constant)
        {
            _constants.Add(constant);
            return _constants.Count - 1;
        }

        public int AddVariable(string name)
        {
            _variables.Add(name);
            return _variables.Count - 1;
        }

        public Bytecode ToBytecode() => new Bytecode(
            code: _code.ToArray(),
            constants: _constants.ToArray(),
            variables: _variables.ToArray());
    }
}
