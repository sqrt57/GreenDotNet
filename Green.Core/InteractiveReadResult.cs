using System.Collections.Generic;

namespace Green
{
    public readonly struct InteractiveReadResult
    {
        public bool Finished { get; }

        public IReadOnlyList<ISyntax> Objects { get; }

        public InteractiveReadResult(bool finished, IReadOnlyList<ISyntax> objects)
        {
            Finished = finished;
            Objects = objects;
        }
    }
}
