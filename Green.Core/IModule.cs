using System.Collections.Generic;

namespace Green
{
    public interface IModule
    {
        string Name { get; }

        IReadOnlyDictionary<string, object> Globals { get; }
    }
}
