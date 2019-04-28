using System.Collections.Generic;

namespace Green
{
    public sealed class BytecodeBuilder
    {
        private List<byte> _code = new List<byte>();
        private List<object> _constants = new List<object>();

        public void AddCode(OpCode code) => _code.Add((byte) code);

        public void AddCode(byte code) => _code.Add(code);

        public int AddConstant(object constant)
        {
            _constants.Add(constant);
            return _constants.Count - 1;
        }

        public Bytecode ToBytecode() => new Bytecode(
            code: _code.ToArray(),
            constants: _constants.ToArray());
    }
}
