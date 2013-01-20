using Methods.ConstantTimeImmutableQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DropStackTest {
    [TestMethod]
    public void PushPopDrop() {
        var r = new DropStack<char>();
        r.IsEmpty.AssertIsTrue();
        r.Count.AssertEquals(0);
        
        r = r.Push('a');
        r.IsEmpty.AssertIsFalse();
        r.Count.AssertEquals(1);
        r.Peek.AssertEquals('a');

        r = r.Push('b').Push('c');
        r.IsEmpty.AssertIsFalse();
        r.Count.AssertEquals(3);
        r.Peek.AssertEquals('c');

        r = r.Pop();
        r.IsEmpty.AssertIsFalse();
        r.Count.AssertEquals(2);
        r.Peek.AssertEquals('b');

        r = r.Drop();
        r.IsEmpty.AssertIsFalse();
        r.Count.AssertEquals(1);
        r.Peek.AssertEquals('b');

        r = r.Drop();
        r.IsEmpty.AssertIsTrue();
        r.Count.AssertEquals(0);
    }
    [TestMethod]
    public void KeepOnly() {
        var r = new DropStack<char>().Push('a').Push('b').Push('c').Push('d').KeepOnly(2);
        r.Count.AssertEquals(2);
        r.Peek.AssertEquals('d');
        r.Pop().Peek.AssertEquals('c');
    }
}
