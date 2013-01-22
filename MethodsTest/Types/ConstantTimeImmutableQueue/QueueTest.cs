using System;
using System.Collections.Immutable;
using Methods.ConstantTimeImmutableQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class QueueTest {
    [TestMethod]
    public void Basic() {
        var q = Queue<int>.Empty;
        q.Count.AssertEquals(0);
        
        q = q.Enqueue(5);
        q.Count.AssertEquals(1);
        q.Peek().AssertEquals(5);

        q = q.Enqueue(6);
        q.Count.AssertEquals(2);
        q.Peek().AssertEquals(5);

        q = q.Dequeue();
        q.Count.AssertEquals(1);
        q.Peek().AssertEquals(6);

        q = q.Dequeue();
        q.Count.AssertEquals(0);
    }
    [TestMethod]
    public void Fuzz() {
        foreach (var seed in 10.Range()) {
            var r = new Random(7231 + seed);
            var refQueue = ImmutableQueue<int>.Empty;
            var q = Queue<int>.Empty;
            foreach (var i in 1000.Range()) {
                if (r.Next(2) == 0 && !refQueue.IsEmpty) {
                    q.Peek().AssertEquals(refQueue.Peek());
                    q = q.Dequeue();
                    refQueue = refQueue.Dequeue();
                } else {
                    var e = r.Next(1000);
                    q = q.Enqueue(e);
                    refQueue = refQueue.Enqueue(e);
                }
            }

            while (!refQueue.IsEmpty) {
                (q.Count > 0).AssertIsTrue();
                q.Peek().AssertEquals(refQueue.Peek());
                q = q.Dequeue();
                refQueue = refQueue.Dequeue();
            }
            q.Count.AssertEquals(0);
        }
    }
}
