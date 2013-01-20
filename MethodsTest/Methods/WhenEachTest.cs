using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Methods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class WhenEachTest {
    [TestMethod]
    public void PropagatesAllTypesOfTasks() {
        var variousTasksRaw = 
            5.Range().Select(e => (Task)Task.FromResult(e))
            .Concat(new[] { (Task)new InvalidOperationException().ToFaultedTask<int>() })
            .Concat(new[] { (Task)TestUtil.CancelledTask<int>() })
            .Concat(new Task[] { null })
            .ToArray();
        variousTasksRaw
            .ToObservable()
            .WhenEach()
            .ToList()
            .ToTask()
            .AssertRanToCompletion()
            .AssertSequenceEquals(variousTasksRaw);

        var variousTasksTyped = 
            5.Range().Select(Task.FromResult)
            .Concat(new[] { new InvalidOperationException().ToFaultedTask<int>() })
            .Concat(new[] { TestUtil.CancelledTask<int>() })
            .Concat(new Task<int>[] {null})
            .ToArray();
        variousTasksTyped
            .ToObservable()
            .WhenEach()
            .ToList()
            .ToTask()
            .AssertRanToCompletion()
            .AssertSequenceEquals(variousTasksTyped);
    }
    [TestMethod]
    public void CompletesOnlyAfterTasksComplete() {
        var s = new TaskCompletionSource<int>();
        var li = new[] { s.Task }
            .ToObservable()
            .Concat(new InvalidOperationException().ToFaultedObservable<Task<int>>())
            .WhenEach()
            .ObserveEventualIntoList();
        li[0].AssertNotCompleted();
        s.SetResult(0);
        li[0].AssertRanToCompletion().AssertRanToCompletion().AssertEquals(0);
        li[1].AssertFailed<InvalidOperationException>();
    }
    [TestMethod]
    public void FaultsOnlyAfterTasksComplete() {
        var s = new TaskCompletionSource<int>();
        var li = new[] {s.Task}
            .ToObservable()
            .WhenEach()
            .ObserveEventualIntoList();
        li[0].AssertNotCompleted();
        s.SetResult(0);
        li[0].AssertRanToCompletion().AssertRanToCompletion().AssertEquals(0);
        li[1].AssertCancelled(); // completed
    }
    [TestMethod]
    public void Concurrent() {
        var overlapping = new AnonymousObservable<Task<int>>(observer => {
            var n = 6;
            var r = n;
            for (var i = 0; i < n; i++)
                new Thread(() => {
                    observer.OnNext(Task.FromResult(0));
                    if (Interlocked.Decrement(ref r) == 0)
                        observer.OnCompleted();
                }).Start();
            return Disposable.Empty;
        });
        
        var overlapCount = 0;
        var t = new TaskCompletionSource<bool>();
        overlapping
            .WhenEach()
            .Subscribe(e => {
                if (Interlocked.Increment(ref overlapCount) > 4) 
                    t.TrySetResult(true);
                t.Task.AssertRanToCompletion();
            });
        t.Task.AssertRanToCompletion();
    }
    [TestMethod]
    public void WorksWithoutCompletion() {
        var hang = new AnonymousObservable<Task<int>>(obs => Disposable.Empty);
        var r = 1.Range()
            .Select(Task.FromResult)
            .ToObservable()
            .Concat(hang)
            .WhenEach()
            .ObserveEventualIntoList();
        r[0].AssertRanToCompletion().AssertRanToCompletion().AssertEquals(0);
        r[1].AssertNotCompleted();
    }
}
