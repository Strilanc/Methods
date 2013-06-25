using System;
using System.Collections.Generic;
using Methods.LazyCount;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LazyCountTest {
    private static IEnumerable<bool> FailAfterNthItem(int n) {
        while (n > 0) {
            n--;
            yield return false;
        }
        throw new Exception();
    }
    private static IEnumerable<bool> LazyN(int n) {
        while (n > 0) {
            n--;
            yield return false;
        }
    }

    [TestMethod]
    public void AdvanceOnlyAsMuchAsNecessary() {
        var e5 = FailAfterNthItem(5);
        AssertUtil.Throws(() => e5.LazyCount().Count);
        AssertUtil.Throws(() => e5.LazyCount().LongCount);

        foreach (var e in new[] {int.MinValue, -1, 0, 1, 2, 3, 4, 5}) {
            if (e != 5) {
                (e5.LazyCount() > e).AssertIsTrue();
                (e5.LazyCount() != e).AssertIsTrue();
                (e5.LazyCount() <= e).AssertIsFalse();
                (e5.LazyCount() == e).AssertIsFalse();
                 e5.LazyCount().Equals(e).AssertIsFalse();
            } else {
                AssertUtil.Throws(() => e5.LazyCount() > e);
                AssertUtil.Throws(() => e5.LazyCount() != e);
                AssertUtil.Throws(() => e5.LazyCount() <= e);
                AssertUtil.Throws(() => e5.LazyCount() == e);
                AssertUtil.Throws(() => e5.LazyCount().Equals(e));
            }
        }
    }
    [TestMethod]
    public void CorrectLazyCount() {
        new int[5].LazyCount().Count.AssertEquals(5);
        new int[3].LazyCount().LongCount.AssertEquals(3);
        LazyN(5).LazyCount().Count.AssertEquals(5);
        LazyN(3).LazyCount().LongCount.AssertEquals(3);

        LazyN(0).LazyCount().Count.AssertEquals(0);

        (LazyN(3).LazyCount() > 4).AssertIsFalse();
        (LazyN(3).LazyCount() > 3).AssertIsFalse();
        (LazyN(3).LazyCount() > 2).AssertIsTrue();

        (LazyN(3).LazyCount() >= 4).AssertIsFalse();
        (LazyN(3).LazyCount() >= 3).AssertIsTrue();
        (LazyN(3).LazyCount() >= 2).AssertIsTrue();

        (LazyN(3).LazyCount() < 4).AssertIsTrue();
        (LazyN(3).LazyCount() < 3).AssertIsFalse();
        (LazyN(3).LazyCount() < 2).AssertIsFalse();

        (LazyN(3).LazyCount() <= 4).AssertIsTrue();
        (LazyN(3).LazyCount() <= 3).AssertIsTrue();
        (LazyN(3).LazyCount() <= 2).AssertIsFalse();

        (LazyN(3).LazyCount() == 2).AssertIsFalse();
        (LazyN(3).LazyCount() == 3).AssertIsTrue();
        (LazyN(3).LazyCount() == 4).AssertIsFalse();

        (LazyN(3).LazyCount() != 2).AssertIsTrue();
        (LazyN(3).LazyCount() != 3).AssertIsFalse();
        (LazyN(3).LazyCount() != 4).AssertIsTrue();
    }
}
