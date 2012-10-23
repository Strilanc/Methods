using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Reactive.Linq;

internal static class TestUtil {
    public static IReadOnlyList<Task<T>> ObserveEventualIntoList<T>(this IObservable<T> items) {
        var li = new List<TaskCompletionSource<T>> { new TaskCompletionSource<T>() };
        var ri = new List<Task<T>> { li[0].Task };
        var syncRoot = new object();
        Func<TaskCompletionSource<T>> f = () => {
            lock (syncRoot) {
                var r = li.Last();
                li.Add(new TaskCompletionSource<T>());
                ri.Add(li.Last().Task);
                return r;
            }
        };
        items.Subscribe(
            e => f().SetResult(e),
            ex => f().SetException(ex),
            () => f().SetCanceled());
        return ri;
    }
    public static IEnumerable<int> Range(this int count) {
        return Enumerable.Range(0, count);
    }
    public static IEnumerable<T> Repeat<T>(this T item, int count) {
        return Enumerable.Repeat(item, count);
    }
    public static Task<T> CancelledTask<T>() {
        var t = new TaskCompletionSource<T>();
        t.SetCanceled();
        return t.Task;
    }
    public static Task<T> ToFaultedTask<T>(this Exception exception) {
        var t = new TaskCompletionSource<T>();
        t.SetException(exception);
        return t.Task;
    }
    public static IObservable<T> ToFaultedObservable<T>(this Exception exception) {
        return new AnonymousObservable<T>(observer => {
            observer.OnError(exception);
            return Disposable.Empty;
        });
    }
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action) {
        foreach (var e in items)
            action(e);
    }
}
