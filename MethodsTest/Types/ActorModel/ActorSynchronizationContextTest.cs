using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Methods.ActorModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ActorSynchronizationContextTest {
    [TestMethod]
    public void PostWorks() {
        var r = new ActorSynchronizationContext();
        var a = new TaskCompletionSource<bool>();
        r.Post(z => a.SetResult(true), null);
        a.Task.AssertRanToCompletion();
    }
    [TestMethod]
    public void NoInterference() {
        var r = new ActorSynchronizationContext();
        var n = 0;
        var t = Task.WhenAll(Enumerable.Range(0, 5).Select(async e => await Task.Factory.StartNew(() => {
            for (var i = 0; i < 500; i++)
                r.Post(z => n += 1, null);
            
            var a = new TaskCompletionSource<bool>();
            r.Post(z => a.SetResult(true), null);
            a.Task.AssertRanToCompletion();
        }, TaskCreationOptions.LongRunning)));
        t.AssertRanToCompletion(timeout: TimeSpan.FromSeconds(20)); // lots of work, long timeout
        n.AssertEquals(500 * 5);
    }
    [TestMethod]
    public void NoOverlap() {
        var r = new ActorSynchronizationContext();
        var n = 0;
        for (var i = 0; i < 1000; i++) {
            r.Post(z => {
                Interlocked.Increment(ref n).AssertEquals(1);
                Interlocked.Decrement(ref n).AssertEquals(0);
            }, null);
        }
        
        var a = new TaskCompletionSource<bool>();
        r.Post(z => a.SetResult(true), null);
        a.Task.AssertRanToCompletion();
    }
}
