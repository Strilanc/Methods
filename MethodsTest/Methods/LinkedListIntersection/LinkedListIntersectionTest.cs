using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Methods.LinkedListIntersection;

[TestClass]
public class LinkedListIntersectionTest {
    [TestMethod]
    public void CanIntersectAtNull() {
        LinkedListIntersectionUtil.FindEarliestIntersection<int>(null, null).AssertEquals(null);
        LinkedListIntersectionUtil.FindEarliestIntersection(null, new Link<int>()).AssertEquals(null);
        new Link<int>().FindEarliestIntersection(null).AssertEquals(null);
    }

    [TestMethod]
    public void CanIntersectTrivialCases() {
        var c = new Link<int>();
        var d = new Link<int> { Next = c };
        c.FindEarliestIntersection(c).AssertEquals(c);
        d.FindEarliestIntersection(c).AssertEquals(c);
        c.FindEarliestIntersection(d).AssertEquals(c);
        
        var d2 = new Link<int> { Next = c };
        d.FindEarliestIntersection(d).AssertEquals(d);
        d.FindEarliestIntersection(d2).AssertEquals(c);
    }

    [TestMethod]
    public void CanIntersectVariousLengths() {
        foreach (var i in 25.Range()) {
            foreach (var j in 25.Range()) {
                foreach (var k in 25.Range()) {
                    // end in common chain or null
                    var c1 = i.Range().ToLinkedList();
                    var p1 = j.Range().ToLinkedList(tail: c1);
                    var p2 = k.Range().ToLinkedList(tail: c1);
                    p1.FindEarliestIntersection(p2).AssertEquals(c1);
                }
            }
        }
    }
}
