using System.Linq;
using Methods.ConstantTimeImmutableQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DropCollectStackTest {
    [TestMethod]
    public void Stability() {
        var x = DropCollectStack<int>.Empty.Push(-3).Push(-2).Push(-1);
        foreach (var i in 100.Range()) {
            x = x.Drop().Push(i);
            x.Count.AssertEquals(3);
            x.UnderlyingStack.UnderlyingStack.Take(3).AssertSequenceEquals(new[] { i, i - 1, i - 2 });
            (x.UnderlyingStack.UnderlyingStack.Count() <= 3 * 6).AssertIsTrue();
        }
    }

    [TestMethod]
    public void Push() {
        var x = DropCollectStack<int>.Empty.Push(4);
        var y = x.Push(5);
        x.Count.AssertEquals(1);
        y.Count.AssertEquals(2);
        x.UnderlyingStack.UnderlyingStack.Take(1).AssertSequenceEquals(new[] { 4 });
        y.UnderlyingStack.UnderlyingStack.Take(2).AssertSequenceEquals(new[] { 5, 4 });
    }

    [TestMethod]
    public void Drop() {
        var x = DropCollectStack<int>.Empty.Push(4).Push(5).Drop();
        x.Count.AssertEquals(1);
        x.UnderlyingStack.UnderlyingStack.Take(1).AssertSequenceEquals(new[] { 5 });
        x.Drop().Count.AssertEquals(0);
    }
}
