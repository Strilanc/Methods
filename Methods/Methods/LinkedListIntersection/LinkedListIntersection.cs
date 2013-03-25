using System;
using System.Collections.Generic;
using System.Linq;

namespace Methods.LinkedListIntersection {
    public sealed class Link<T> {
        public Link<T> Next;
        public T Value;
    }

    public static class LinkedListIntersectionUtil {
        public static Link<T> FindEarliestIntersection<T>(this Link<T> h0, Link<T> h1) {
            // find *any* intersection, and the distances to it
            var node = new[] {h0, h1};
            var dist = new[] {0, 0};
            var stepSize = 1;
            while (node[0] != node[1]) {
                // advance each node progressively farther, watching for the other node
                foreach (var i in Enumerable.Range(0, 2)) {
                    foreach (var repeat in Enumerable.Range(0, stepSize)) {
                        if (node[i] == null) break;
                        if (node[0] == node[1]) break;
                        node[i] = node[i].Next;
                        dist[i] += 1;
                    }
                    stepSize *= 2;
                }
            }

            // align heads to be an equal distance from the intersection
            var r = dist[1] - dist[0];
            while (r < 0) {
                h0 = h0.Next;
                r += 1;
            }
            while (r > 0) {
                h1 = h1.Next;
                r -= 1;
            }

            // advance heads until they intersect (at the first intersection)
            while (h0 != h1) {
                h0 = h0.Next;
                h1 = h1.Next;
            }

            return h0;
        }

        public static Link<T> ToLinkedList<T>(this IEnumerable<T> items, Link<T> tail = null) {
            if (items == null) throw new ArgumentNullException("items");
            return items.Reverse().Aggregate(tail, (current, e) => new Link<T> {Next = current, Value = e});
        }
    }
}
