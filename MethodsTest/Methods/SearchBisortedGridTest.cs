using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Methods;

static class SearchBisortedGridTestUtil {
    public static void AssertCanFindAllButNot<T>(this IReadOnlyList<IReadOnlyList<T>> items, params T[] excluded) {
        foreach (var item in items) {
            foreach (var e in item) {
                var r = items.TryFindItemInSortedMatrix(e);
                (r != null).AssertIsTrue();
                items[r.Item1][r.Item2].AssertEquals(e);
            }
        }

        foreach (var e in excluded) {
            items.TryFindItemInSortedMatrix(e).AssertEquals(null);
        }
    }
}

[TestClass]
public class SearchBisortedGridTest {
    [TestMethod]
    public void TrivialTest() {
        new int[0][].AssertCanFindAllButNot(0);
        new[] { new[] { 1 } }.AssertCanFindAllButNot(0, 2);
        new[] { new[] { 1, 2 }, new[] { 3, 4 } }.AssertCanFindAllButNot(0, 5);
        new[] { new[] { 1, 2 }, new[] { 2, 3 } }.AssertCanFindAllButNot(0, 5);
        new[] { new[] { 10, 20 }, new[] { 30, 40 } }.AssertCanFindAllButNot(5, 15, 25, 35, 45);
        
        100.Range().Select(e => Enumerable.Range(e * 10, 10).Select(i => i * 2).ToArray()).ToArray()
            .AssertCanFindAllButNot(1,101,201);
        10.Range().Select(e => Enumerable.Range(e * 100, 100).Select(i => i * 2).ToArray()).ToArray()
            .AssertCanFindAllButNot(1,101,201);
    }

    [TestMethod]
    public void PerturbedTest() {
        const int Repetitions = 100;
        var rng = new Random(235711);
        foreach (var width in new[] { 5, 36 }) {
            foreach (var height in new[] { 5, 50, 150 }) {
                foreach (var _ in Repetitions.Range()) {
                    var matrix = width.Range().Select(e => new int[height]).ToArray();
                    matrix[0][0] = rng.Next(5);
                    foreach (var r in height.Range().Skip(1)) {
                        matrix[0][r] = matrix[0][r - 1] + rng.Next(5);
                    }
                    foreach (var c in width.Range().Skip(1)) {
                        matrix[c][0] = matrix[c - 1][0] + rng.Next(5);
                    }
                    foreach (var r in height.Range().Skip(1)) {
                        foreach (var c in width.Range().Skip(1)) {
                            matrix[c][r] = Math.Max(matrix[c - 1][r], matrix[c][r - 1]) + rng.Next(5);
                        }
                    }

                    var target = matrix[rng.Next(width)][rng.Next(height)];
                    var index = matrix.TryFindItemInSortedMatrix(target);
                    (index != null).AssertIsTrue();
                    matrix[index.Item1][index.Item2].AssertEquals(target);
                }
            }
        }
    }
}
