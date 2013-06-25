using System;
using System.Collections;
using System.Diagnostics;

namespace Methods.LazyCount {
    [DebuggerDisplay("{ToString()}")]
    public struct LazyCounter : IComparable<LazyCounter>, IEquatable<LazyCounter> {
        ///<remarks>Store instance data indirectly, to safely allow both mutable data AND a non-null default value.</remarks>>
        private readonly InstanceData _data;

        private sealed class InstanceData {
            public long CountSoFar;
            ///<remarks>Set to null when done advancing.</remarks>>
            public IEnumerator CountAdvancer;
        }

        private bool HasFinishedCounting { get { return _data == null || _data.CountAdvancer == null; } }
        private long CountSoFar { get { return _data == null ? 0 : _data.CountSoFar; } }

        ///<summary>Constructs a lazy counter with an initial count and an optional enumerator to count more items from.</summary>
        public LazyCounter(long countSoFar = 0, IEnumerator countAdvancer = null) {
            this._data = new InstanceData {
                CountSoFar = countSoFar,
                CountAdvancer = countAdvancer
            };
        }

        ///<summary>Advances until the count advancer has finished or the count so far is over the limit.</summary>
        private void TryAdvancePast(long limit) {
            while (CountSoFar <= limit && !HasFinishedCounting) {
                if (_data.CountAdvancer.MoveNext()) {
                    checked {
                        _data.CountSoFar++;
                    }
                } else {
                    _data.CountAdvancer = null;
                }
            }
        }

        /// <summary>
        /// Compares the eventual final values of two lazy counters without advancing more than necessary.
        /// Advances both until one of them stops and the other has attempted to pass.
        /// </summary>
        private static int Compare(LazyCounter leftHandSide, LazyCounter rightHandSide) {
            while (true) {
                var finalAttemptToPass = leftHandSide.HasFinishedCounting || rightHandSide.HasFinishedCounting;
                rightHandSide.TryAdvancePast(leftHandSide.CountSoFar);
                leftHandSide.TryAdvancePast(rightHandSide.CountSoFar);
                if (finalAttemptToPass) break;
            }
            return leftHandSide.CountSoFar.CompareTo(rightHandSide.CountSoFar);
        }
        /// <summary>
        /// Determines if one lazy counter's eventual final value will be less than the eventual final value of another.
        /// Advances both until one of them stops and the other has attempted to pass or match.
        /// </summary>
        private static bool IsLessThan(LazyCounter leftHandSide, LazyCounter rightHandSide) {
            while (true) {
                var finalAttemptToMatchOrPass = leftHandSide.HasFinishedCounting || rightHandSide.HasFinishedCounting;
                rightHandSide.TryAdvancePast(leftHandSide.CountSoFar); // pass
                leftHandSide.TryAdvancePast(rightHandSide.CountSoFar - 1); // match
                if (finalAttemptToMatchOrPass) break;
            }
            return leftHandSide.CountSoFar < rightHandSide.CountSoFar;
        }

        ///<summary>The final count, after advancing the enumerator to count until it has finished.</summary>
        public long LongCount {
            get {
                TryAdvancePast(long.MaxValue);
                return CountSoFar;
            }
        }
        ///<summary>The final count, after advancing the enumerator to count until it has finished.</summary>
        public int Count {
            get {
                checked {
                    return (int)LongCount;
                }
            }
        }

        ///<summary>Compares the eventual final count of this counter to another.</summary>
        public int CompareTo(LazyCounter other) {
            return Compare(this, other);
        }
        ///<summary>Compares the eventual final count of this counter to another.</summary>
        public override bool Equals(object obj) {
            return obj is LazyCounter && Equals((LazyCounter)obj);
        }
        ///<summary>Compares the eventual final count of this counter to another.</summary>
        public bool Equals(LazyCounter other) {
            return this.CompareTo(other) == 0;
        }
        ///<summary>A hash code of the eventual final count of this counter.</summary>
        public override int GetHashCode() {
            return LongCount.GetHashCode();
        }
        ///<summary>A string representation of the current state of the lazy counter.</summary>
        public override string ToString() {
            return string.Format(
                "{0}{1}",
                HasFinishedCounting ? "=" : ">=",
                CountSoFar);
        }

        ///<summary>A counter containing a final count equal to the converted value.</summary>
        public static implicit operator LazyCounter(int value) {
            return new LazyCounter(value);
        }
        ///<summary>A counter containing a final count equal to the converted value.</summary>
        public static implicit operator LazyCounter(long value) {
            return new LazyCounter(value);
        }
        ///<summary>A counter with a the given final value.</summary>
        public static implicit operator long(LazyCounter counter) {
            return counter.LongCount;
        }

        ///<summary>Compares the eventual final count of two counters.</summary>
        public static bool operator ==(LazyCounter leftHandSide, LazyCounter rightHandSide) {
            return Compare(leftHandSide, rightHandSide) == 0;
        }
        ///<summary>Compares the eventual final count of two counters.</summary>
        public static bool operator !=(LazyCounter leftHandSide, LazyCounter rightHandSide) {
            return Compare(leftHandSide, rightHandSide) != 0;
        }
        ///<summary>Compares the eventual final count of two counters.</summary>
        public static bool operator <=(LazyCounter leftHandSide, LazyCounter rightHandSide) {
            return !IsLessThan(rightHandSide, leftHandSide);
        }
        ///<summary>Compares the eventual final count of two counters.</summary>
        public static bool operator >=(LazyCounter leftHandSide, LazyCounter rightHandSide) {
            return !IsLessThan(leftHandSide, rightHandSide);
        }
        ///<summary>Compares the eventual final count of two counters.</summary>
        public static bool operator <(LazyCounter leftHandSide, LazyCounter rightHandSide) {
            return IsLessThan(leftHandSide, rightHandSide);
        }
        ///<summary>Compares the eventual final count of two counters.</summary>
        public static bool operator >(LazyCounter leftHandSide, LazyCounter rightHandSide) {
            return IsLessThan(rightHandSide, leftHandSide);
        }
    }
}
