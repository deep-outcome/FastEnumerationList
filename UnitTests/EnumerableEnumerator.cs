using System.Collections;
using System.Collections.Generic;

namespace UnitTests;

sealed class EnumerableEnumerator<T> ( IEnumerator<T> enumerator ) : IEnumerable<T>
{
  public IEnumerator<T> GetEnumerator () => enumerator;
  IEnumerator IEnumerable.GetEnumerator () => enumerator;
}
