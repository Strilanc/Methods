using System;
using System.Collections.Immutable;

namespace Methods.ConstantTimeImmutableQueue {
    /// <summary>
    /// A constant-time immutable stack paired with a count.
    /// Items on the bottom of the stack can 'dropped', which ignores them by decrementing the count.
    /// Dropped items are still in the underlying stack, and can't be garbage collected, but are ignored.
    /// </summary>
    internal struct DropStack<T> {
        public static readonly DropStack<T> Empty = default(DropStack<T>);

        private readonly ImmutableStack<T> _stack;
        private ImmutableStack<T> Stack { get { return _stack ?? ImmutableStack<T>.Empty; } }
        private readonly int _count;

        private DropStack(ImmutableStack<T> stack, int count) {
            this._stack = count == 0 ? null : stack;
            this._count = count;
        }

        ///<summary>The number of undropped items in the stack.</summary>
        public int Count { get { return _count; } }

        ///<summary>Whether or not the stack has no undropped items.</summary>
        public bool IsEmpty { get { return Count == 0; } }
        ///<summary>The item on the top of the stack.</summary>
        public T Peek {
            get {
                if (IsEmpty) throw new InvalidOperationException("Empty");
                return Stack.Peek();
            }
        }

        ///<summary>A modified copy with an item added to the top of the stack.</summary>
        public DropStack<T> Push(T value) {
            return new DropStack<T>(Stack.Push(value), _count + 1);
        }
        ///<summary>A modified copy with an item removed from the top of the stack.</summary>
        public DropStack<T> Pop() {
            if (IsEmpty) throw new InvalidOperationException("Empty");
            if (Count == 1) return Empty;
            return new DropStack<T>(Stack.Pop(), _count - 1);
        }
        ///<summary>A modified copy with an item ignored from the bottom of the stack.</summary>
        public DropStack<T> Drop() {
            if (IsEmpty) throw new InvalidOperationException("Empty");
            if (Count == 1) return Empty;
            return new DropStack<T>(Stack, _count - 1);
        }
        ///<summary>A modified copy ignoring everything except the given number of items from the top of the stack.</summary>
        public DropStack<T> KeepOnly(int keptCount) {
            if (keptCount < 0) throw new ArgumentOutOfRangeException("keptCount", "keptCount < 0");
            if (keptCount > Count) throw new ArgumentOutOfRangeException("keptCount", "keptCount > Count");
            return new DropStack<T>(Stack, keptCount);
        }
    }
}
