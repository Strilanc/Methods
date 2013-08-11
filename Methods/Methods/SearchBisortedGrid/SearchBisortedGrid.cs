﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Methods {
    public static class SearchSortedGridUtil {
        private sealed class AnonymousReadOnlyList<T> : IReadOnlyList<T> {
            private readonly Func<int> _counter;
            private readonly Func<int, T> _getter;
            public AnonymousReadOnlyList(Func<int> counter, Func<int, T> getter) {
                _counter = counter;
                _getter = getter;
            }
            public IEnumerator<T> GetEnumerator() {
                for (var i = 0; i < _counter(); i++) {
                    yield return _getter(i);
                }
            }
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
            public int Count { get { return _counter(); } }
            public T this[int index] { get { return _getter(index); } }
        }
        private static IReadOnlyList<IReadOnlyList<T>> Transpose<T>(this IReadOnlyList<IReadOnlyList<T>> rectangle) {
            if (rectangle.Count == 0) return rectangle;
            return new AnonymousReadOnlyList<IReadOnlyList<T>>(
                () => rectangle[0].Count,
                i => new AnonymousReadOnlyList<T>(
                    () => rectangle.Count,
                    j => rectangle[j][i]));
        }
        public static Tuple<int, int> TryFindIndexOfBisortedItem<T>(this IReadOnlyList<IReadOnlyList<T>> grid, T item, IComparer<T> comparer = null) {
            if (grid == null) throw new ArgumentNullException("grid");
            comparer = comparer ?? Comparer<T>.Default;

            // check size
            var width = grid.Count;
            if (width == 0) return null;
            var height = grid[0].Count;
            if (height < width) {
                var result = grid.Transpose().TryFindIndexOfBisortedItem(item, comparer);
                if (result == null) return null;
                return Tuple.Create(result.Item2, result.Item1);
            }

            // search
            var minCol = 0;
            var maxRow = height - 1;
            var t = height / width;
            while (minCol < width && maxRow >= 0) {
                // query the item in the minimum column, t above the maximum row
                var luckyRow = Math.Max(maxRow - t, 0);
                var cmpItemVsLucky = comparer.Compare(item, grid[minCol][luckyRow]);
                if (cmpItemVsLucky == 0) return Tuple.Create(minCol, luckyRow);

                // did we eliminate t rows from the bottom?
                if (cmpItemVsLucky < 0) {
                    maxRow = luckyRow - 1;
                    continue;
                }

                // we eliminated most of the current minimum column
                // spend lg(t) time eliminating rest of column
                var minRowInCol = luckyRow + 1;
                var maxRowInCol = maxRow;
                while (minRowInCol <= maxRowInCol) {
                    var mid = minRowInCol + (maxRowInCol - minRowInCol + 1) / 2;
                    var cmpItemVsMid = comparer.Compare(item, grid[minCol][mid]);
                    if (cmpItemVsMid == 0) return Tuple.Create(minCol, mid);
                    if (cmpItemVsMid > 0) {
                        minRowInCol = mid + 1;
                    } else {
                        maxRowInCol = mid - 1;
                        maxRow = mid - 1;
                    }
                }

                minCol += 1;
            }

            return null;
        }
    }
}
