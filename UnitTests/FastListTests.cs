using FastEnumerationList;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace UnitTests;

[TestClass]
public class FastListTests
{
  [TestMethod]
  public void GetEnumerator_ListDeclared_GotListEnumerator ()
  {
    List<int> listDeclared = new FastList<int>();

    Assert.AreEqual (GetListEnumeratorType<int> (), listDeclared.GetEnumerator ().GetType ());
  }

  [TestMethod]
  public void GetEnumerator_FastListDeclared_GotFastListEnumerator ()
  {
    var fastListDeclared = new FastList<int>();

    Assert.AreNotEqual (GetListEnumeratorType<int> (), fastListDeclared.GetEnumerator ().GetType ());
    Assert.AreEqual (typeof (FastList<int>.FastEnumerator), fastListDeclared.GetEnumerator ().GetType ());
  }

  [TestMethod]
  public void GetEnumerator_ProperlyEnumerates ()
  {
    IEnumerable<int> source = Enumerable.Range(0, 1000);
    FastList<int> fastListDeclared = new (1_000_000);
    fastListDeclared.AddRange (source);

    EnumerableEnumerator<int> enumerable = new (fastListDeclared.GetEnumerator());

    List<int> listDeclared = new FastList<int>();
    listDeclared.AddRange (source);

    Assert.IsTrue (enumerable.SequenceEqual (listDeclared));
  }

  static Type GetListEnumeratorType<T> () => new List<T> ().GetEnumerator ().GetType ();


  [TestMethod]
  public void PerfTest ()
  {
    //int testCycles = 500_000;
    int testCycles = 500;//_000;

    var listTimes_Foreach = new List<TimeSpan>();
    var fastListTimes_Foreach = new List<TimeSpan>();
    var arrayTimes_Foreach = new List<TimeSpan>();

    var listTimes_For = new List<TimeSpan>();
    var fastListTimes_For = new List<TimeSpan>();
    var arrayTimes_For = new List<TimeSpan>();

    var arrayTimes_For_Unsafe = new List<TimeSpan>();

    long[] numbers = [.. Enumerable
        .Range(0, 500_000)
        .Select(Convert.ToInt64)];

    long refSum = checked(numbers.Sum());
    var stopWatch = new Stopwatch();

    var testList = new List<long>(numbers);
    var testFastList = new FastList<long>(numbers);

    while (testCycles-- > 0)
    {
      TestForeach (testList, listTimes_Foreach, stopWatch, refSum);
      TestForeach (testFastList, fastListTimes_Foreach, stopWatch, refSum);
      TestForeach (numbers, arrayTimes_Foreach, stopWatch, refSum);

      TestFor (testList, listTimes_For, stopWatch, refSum);
      TestFor (testFastList, fastListTimes_For, stopWatch, refSum);
      TestFor_Array (numbers, arrayTimes_For, stopWatch, refSum);

      TestFor_Array_Unsafe (numbers, arrayTimes_For_Unsafe, stopWatch, refSum);
    }

    DiscardMinMaxValues (listTimes_Foreach);
    DiscardMinMaxValues (fastListTimes_Foreach);
    DiscardMinMaxValues (arrayTimes_Foreach);

    DiscardMinMaxValues (listTimes_For);
    DiscardMinMaxValues (fastListTimes_For);
    DiscardMinMaxValues (arrayTimes_For);

    DiscardMinMaxValues (arrayTimes_For_Unsafe);

    TimeSpan listTimes_Foreach_Sum = Sum(listTimes_Foreach);
    TimeSpan fastListTimes_Foreach_Sum = Sum(fastListTimes_Foreach);
    TimeSpan array_Foreach_Sum = Sum(arrayTimes_Foreach);

    TimeSpan listTimes_For_Sum = Sum(listTimes_For);
    TimeSpan fastListTimes_For_Sum = Sum(fastListTimes_For);
    TimeSpan array_For_Sum = Sum(arrayTimes_For);

    TimeSpan array_For_Unsafe_Sum = Sum(arrayTimes_For_Unsafe);

    var strBuilder = new StringBuilder();

    _ = strBuilder.AppendLine ($"{nameof (listTimes_Foreach)} {listTimes_Foreach_Sum}");
    _ = strBuilder.AppendLine ($"{nameof (fastListTimes_Foreach)} {fastListTimes_Foreach_Sum}");
    _ = strBuilder.AppendLine ($"{nameof (arrayTimes_Foreach)} {array_Foreach_Sum}");

    _ = strBuilder.AppendLine ($"{nameof (listTimes_For)} {listTimes_For_Sum}");
    _ = strBuilder.AppendLine ($"{nameof (fastListTimes_For)} {fastListTimes_For_Sum}");
    _ = strBuilder.AppendLine ($"{nameof (arrayTimes_For)} {array_For_Sum}");

    _ = strBuilder.AppendLine ($"{nameof (arrayTimes_For_Unsafe)} {array_For_Unsafe_Sum}");
    _ = strBuilder.AppendLine ();

    string result = strBuilder.ToString();

    File.AppendAllText ("./res.txt", result);

    Debug.Write (result);

    Assert.IsGreaterThan (fastListTimes_Foreach_Sum, listTimes_Foreach_Sum);
    Assert.IsGreaterThan (fastListTimes_Foreach_Sum, array_Foreach_Sum);
  }

  static void DiscardMinMaxValues ( List<TimeSpan> timeSpans )
  {
    if (timeSpans.Count == 0) { return; }

    TimeSpan max = timeSpans.Max();
    TimeSpan min = timeSpans.Min();

    Debug.WriteLine ($"Max {max}, min {min}.");

    Remove (max);
    Remove (min);

    void Remove ( TimeSpan timeSpan ) => timeSpans.RemoveAt (timeSpans.IndexOf (timeSpan));
  }

  static TimeSpan Sum ( List<TimeSpan> timeSpans ) => timeSpans.Aggregate (new TimeSpan (0, 0, 0), ( acc, ts ) => acc.Add (ts));

  static void TestForeach ( IEnumerable<long> list, List<TimeSpan> times, Stopwatch stopwatch, long refSum )
  {

    long sum = 0L;
    stopwatch.Restart ();

    foreach (long n in list)
    {
      sum += n;
    }

    stopwatch.Stop ();
    times.Add (stopwatch.Elapsed);

    Assert.AreEqual (refSum, sum);
  }

  static void TestFor ( List<long> list, List<TimeSpan> times, Stopwatch stopwatch, long refSum )
  {

    long sum = 0L;
    stopwatch.Restart ();

    for (int i = 0; i < list.Count; ++i)
    {
      sum += list [i];
    }

    stopwatch.Stop ();
    times.Add (stopwatch.Elapsed);

    Assert.AreEqual (refSum, sum);
  }

  static void TestFor_Array ( long [] list, List<TimeSpan> times, Stopwatch stopwatch, long refSum )
  {

    long sum = 0L;
    stopwatch.Restart ();

    for (int i = 0; i < list.Length; ++i)
    {
      sum += list [i];
    }

    stopwatch.Stop ();
    times.Add (stopwatch.Elapsed);

    Assert.AreEqual (refSum, sum);
  }

  static unsafe void TestFor_Array_Unsafe ( long [] list, List<TimeSpan> times, Stopwatch stopwatch, long refSum )
  {

    long sum = 0L;
    stopwatch.Restart ();

    fixed (long* l = &list [0])
    {
      for (int i = 0; i < list.Length; ++i)
      {
        sum += l [i];
      }
    }

    stopwatch.Stop ();
    times.Add (stopwatch.Elapsed);

    Assert.AreEqual (refSum, sum);
  }

}
