using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Methods.ConstantTimeImmutableQueue {
    /// <summary>An immutable queue of items with guaranteed constant time Enqueue/Dequeue/Peek operations.</summary>
    public sealed class Queue<T> : IEnumerable<T> {
        public static readonly Queue<T> Empty = new Queue<T>(DropCollectStack<T>.Empty, DropStack<T>.Empty, DropStack<T>.Empty, DropStack<T>.Empty);

        private readonly DropCollectStack<T> _incoming;
        private readonly DropStack<T> _outgoing;

        private readonly DropStack<T> _incomingTraverser;
        private readonly DropStack<T> _partialOutgoing;

        private Queue(DropCollectStack<T> incoming,
                      DropStack<T> outgoing,
                      DropStack<T> partialOutgoing,
                      DropStack<T> incomingTraverser) {
            this._incoming = incoming;
            this._outgoing = outgoing;
            this._partialOutgoing = partialOutgoing;
            this._incomingTraverser = incomingTraverser;
        }
        private Queue<T> With(DropCollectStack<T> items = null,
                              DropStack<T>? ready = null,
                              DropStack<T>? reStack = null,
                              DropStack<T>? reFeed = null) {
            return new Queue<T>(
                items ?? _incoming,
                ready ?? _outgoing,
                reStack ?? _partialOutgoing,
                reFeed ?? _incomingTraverser);
        }

        ///<summary>The number of items in the queue.</summary>
        public int Count {
            get {
                return _incoming.Count;
            }
        }

        ///<summary>The item at the front of the queue.</summary>
        public T Peek() {
            if (Count == 0) throw new InvalidOperationException();
            return _outgoing.Peek;
        }

        ///<summary>A modified copy with an item added to the back of the queue.</summary>
        public Queue<T> Enqueue(T value) {
            if (Count == 0) return With(items: _incoming.Push(value), ready: _outgoing.Push(value), reFeed: _incomingTraverser.Push(value));
            return With(items: _incoming.Push(value)).IterRebuild();
        }
        ///<summary>A modified copy with an item removed from the front of the queue.</summary>
        public Queue<T> Dequeue() {
            if (Count == 0) throw new InvalidOperationException();
            var r = _outgoing.Count * 2 <= _incoming.Count + 1 || _incomingTraverser.Count == 0
                  ? IterRebuild().IterRebuild()
                  : this;
            return r.With(items: r._incoming.Drop(), ready: r._outgoing.Pop(), reFeed: r._incomingTraverser.Drop());
        }

        ///<summary>A modified copy with some more rebuilding work done.</summary>
        private Queue<T> IterRebuild() {
            switch (_incomingTraverser.Count) {
                case 0: return With(ready: _partialOutgoing, reStack: DropStack<T>.Empty, reFeed: _incoming.UnderlyingStack);
                case 1: return With(ready: _partialOutgoing.Push(_incomingTraverser.Peek), reStack: DropStack<T>.Empty, reFeed: _incoming.UnderlyingStack);
                default: return With(reStack: _partialOutgoing.Push(_incomingTraverser.Peek), reFeed: _incomingTraverser.Pop());
            }
        }

        public IEnumerator<T> GetEnumerator() {
            for (var e = this; e.Count > 0; e = e.Dequeue())
                yield return e.Peek();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
