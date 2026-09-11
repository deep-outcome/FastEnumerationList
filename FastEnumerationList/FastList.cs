using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FastEnumerationList;

public class FastList<T> : List<T>, IEnumerable<T>
{
  static readonly FieldInfo _itemsInfo = typeof(List<T>).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(f => f.Name == "_items");

  public T [] Items => (T []) _itemsInfo.GetValue (this);

  public FastList () { }

  public FastList ( IEnumerable<T> collection ) : base (collection) { }

  public FastList ( int capacity ) : base (capacity) { }

  new public IEnumerator<T> GetEnumerator () => new FastEnumerator (Items, Count);
  IEnumerator IEnumerable.GetEnumerator () => ((IEnumerable<T>) this).GetEnumerator ();

  sealed public class FastEnumerator ( T [] items, int count ) : IEnumerator<T>
  {
    readonly T[] items = items;
    readonly int count = count;
    int index;

    public void Dispose () { }

    public bool MoveNext ()
    {
      int count = this.count;
      return ++index < count;
    }

    public T Current => items [index];

    object IEnumerator.Current => Current;

    void IEnumerator.Reset () => index = 0;

  }
}
