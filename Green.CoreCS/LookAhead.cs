using System.Collections.Generic;

namespace Green
{
    public static class LookAhead
    {
        public static LookAheadEnumerator<T> Enumerate<T>(IEnumerable<T> source) => new LookAheadEnumerator<T>(source);
    }
}
