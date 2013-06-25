using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

internal static class AssertUtil {
    public static void Throws<T>(Action action) where T : Exception {
        try {
            action();
            Assert.Fail("Expected an exception of type {0} but none was thrown.", typeof (T).FullName);
        } catch (T) {
            // good
        }
    }
    public static void Throws<T>(Func<T> func) {
        try {
            func();
            Assert.Fail("Expected an exception of type {0} but none was thrown.", typeof(Exception).FullName);
        }
        catch (Exception) {
            // good
        }
    }
    [DebuggerStepThrough]
    public static void AssertEquals<T>(this T expected, T actual) {
        Assert.AreEqual(actual, expected);
    }
    [DebuggerStepThrough]
    public static void AssertSequenceEquals<T>(this IEnumerable<T> v1, IEnumerable<T> v2) {
        Assert.IsTrue(v1.SequenceEqual(v2));
    }
    [DebuggerStepThrough]
    public static void AssertSequenceEquals<T>(this IEnumerable<T> v1, params T[] v2) {
        Assert.IsTrue(v1.SequenceEqual(v2));
    }
    [DebuggerStepThrough]
    public static void AssertSetEquals<T>(this IEnumerable<T> v1, IEnumerable<T> v2) {
        var h1 = new HashSet<T>(v1);
        var h2 = new HashSet<T>(v2);
        h1.Count.AssertEquals(h2.Count);
        h1.Intersect(h2).Count().AssertEquals(h1.Count);
    }
    [DebuggerStepThrough]
    public static void AssertIsTrue(this bool b) {
        Assert.IsTrue(b);
    }
    [DebuggerStepThrough]
    public static void AssertIsFalse(this bool b) {
        Assert.IsFalse(b);
    }

    private static void TestWait(this Task t, TimeSpan timeout) {
        Task.WhenAny(t, Task.Delay(timeout)).Wait();
        t.Wait(new TimeSpan(1));
    }
    public static Exception Collapse(this AggregateException exception) {
        if (exception == null) throw new ArgumentNullException("exception");
        var f = exception.Flatten();
        return f.InnerExceptions.Count == 1 
             ? f.InnerExceptions.Single() 
             : f;
    }
    [DebuggerStepThrough]
    public static void AssertRanToCompletion(this Task t, TimeSpan? timeout = null) {
        t.TestWait(timeout ?? TimeSpan.FromSeconds(5));
        t.IsCompleted.AssertIsTrue();
    }
    [DebuggerStepThrough]
    public static T AssertRanToCompletion<T>(this Task<T> t, TimeSpan? timeout = null) {
        t.TestWait(timeout ?? TimeSpan.FromSeconds(5));
        t.IsCompleted.AssertIsTrue();
        return t.Result;
    }
    [DebuggerStepThrough]
    public static void AssertFailed<TException>(this Task t, TimeSpan? timeout = null) where TException : Exception {
        try {
            t.TestWait(timeout ?? TimeSpan.FromSeconds(5));
            Assert.Fail("Expected an exception of type " + typeof(TException).FullName + ", but ran succesfully.");
        } catch (TException) {
        } catch (Exception ax) {
            var ex = ax is AggregateException ? ((AggregateException)ax).Collapse() : ax;
            if (ex is TException) return;
            Assert.Fail("Expected an exception of type " + typeof(TException).FullName + ", but got a different one: " + ex);
        }
    }
    [DebuggerStepThrough]
    public static void AssertCancelled(this Task t, TimeSpan? timeout = null) {
        try {
            t.TestWait(timeout ?? TimeSpan.FromSeconds(5));
        } catch (Exception) {
        }
        t.IsCanceled.AssertIsTrue();
    }
    [DebuggerStepThrough]
    public static void AssertNotCompleted(this Task t, TimeSpan? timeout = null) {
        t.TestWait(timeout ?? TimeSpan.FromMilliseconds(50));
        t.IsCompleted.AssertIsFalse();
        t.IsFaulted.AssertIsFalse();
        t.IsCanceled.AssertIsFalse();
    }
}
