using System;
using System.Collections.Generic;

namespace Green
{
    public sealed class ReadonlyModule : IModule
    {
        public ReadonlyModule(string name, IReadOnlyDictionary<string, object> globals)
        {
            Name = name;
            Globals = globals;
        }

        public string Name { get; }

        public IReadOnlyDictionary<string, object> Globals { get; }
    }
}
