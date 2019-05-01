using System;
using System.Collections.Generic;
using System.Text;

namespace Green
{
    public sealed class LookAheadEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly List<T> _next = new List<T>();

        public LookAheadEnumerator(IEnumerable<T> source)
        {
            _enumerator = source.GetEnumerator();
        }

        public T Next(int index = 0)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index should be non-negative");

            if (!EnsureNext(index + 1))
                throw new InvalidOperationException("No more elements in enumerable");

            return _next[index];
        }

        public bool HasNext(int index = 0)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index should be non-negative");

            return EnsureNext(index + 1);
        }

        public void Advance(int number = 1)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException(nameof(number), number, "Number of items to advance should be non-negative");

            if (!EnsureNext(number))
                throw new InvalidOperationException("No more elements in enumerable");

            _next.RemoveRange(0, number);
        }

        private bool EnsureNext(int number)
        {
            for (int i = 0; i < number - _next.Count; i++)
            {
                if (!_enumerator.MoveNext())
                    return false;
                _next.Add(_enumerator.Current);
            }
            return true;
        }
    }
}
