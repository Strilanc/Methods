using System;
using System.Collections;
using System.Collections.Generic;

namespace Methods.LazyCount {
    public static class LazyCountUtil {
        ///<summary>Counts the number of elements in a sequence, when needed.</summary>
        public static LazyCounter LazyCount(this IEnumerable sequence) {
            if (sequence == null) throw new ArgumentNullException("sequence");

            var asCollection = sequence as ICollection;
            if (asCollection != null) return asCollection.Count;

            return new LazyCounter(countAdvancer: sequence.GetEnumerator());
        }

        ///<summary>Counts the number of elements in a sequence, when needed.</summary>
        public static LazyCounter LazyCount<T>(this IEnumerable<T> sequence) {
            if (sequence == null) throw new ArgumentNullException("sequence");

            var asReadOnlyCollection = sequence as IReadOnlyCollection<T>;
            if (asReadOnlyCollection != null) return asReadOnlyCollection.Count;

            return ((IEnumerable)sequence).LazyCount();
        }
    }
}
