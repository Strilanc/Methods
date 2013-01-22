using System;
using System.Collections.Immutable;

namespace Methods.ConstantTimeImmutableQueue {
    /// <summary>
    /// A constant-time immutable stack paired with a count.
    /// Items on the bottom of the stack can 'dropped', which ignores them by decrementing the count.
    /// Dropped items are eventually dereferenced by later copy-modified versions of the stack, so they can be garbage collected.
    /// </summary>
    public sealed class DropCollectStack<T> {
        public static readonly DropCollectStack<T> Empty = new DropCollectStack<T>();

        private readonly DropStack<T> _items;

        private readonly DropStack<T> _itemsTraverser;
        private readonly ImmutableStack<T> _partialReversedItems;
        private readonly DropStack<T> _partialRebuiltItems;

        private DropCollectStack(DropStack<T> items = default(DropStack<T>),
                                 DropStack<T> itemsTraverser = default(DropStack<T>),
                                 ImmutableStack<T> partialReversedItems = null,
                                 DropStack<T> partialRebuiltItems = default(DropStack<T>)) {
            this._items = items;
            this._itemsTraverser = itemsTraverser;
            this._partialReversedItems = partialReversedItems ?? ImmutableStack<T>.Empty;
            this._partialRebuiltItems = partialRebuiltItems;
        }
        private DropCollectStack<T> With(DropStack<T>? items = null,
                                         DropStack<T>? itemsTraverser = null,
                                         ImmutableStack<T> partialReversedItems = null,
                                         DropStack<T>? partialRebuiltItems = null) {
            return new DropCollectStack<T>(items ?? _items,
                                           itemsTraverser ?? _itemsTraverser,
                                           partialReversedItems ?? _partialReversedItems,
                                           partialRebuiltItems ?? _partialRebuiltItems);
        }

        ///<summary>The number of items in the queue.</summary>
        public int Count { get { return _items.Count; } }
        ///<summary>The items in the queue, in FIFO order.</summary>
        public DropStack<T> UnderlyingStack { get { return _items; } }

        ///<summary>A modified copy with more incremental rebuilding performed (in order to eventually discard garbage).</summary>
        private DropCollectStack<T> IterRebuild() {
            return
                // iteratively transfer from _itemsTraverser to _partialReversedItems
                !_itemsTraverser.IsEmpty ? With(
                    itemsTraverser: _itemsTraverser.Pop(), 
                    partialReversedItems: _partialReversedItems.Push(_itemsTraverser.Peek))
                // then iteratively transfer from _partialReversedItems to _partialRebuiltItems
              : !_partialReversedItems.IsEmpty ? With(
                    partialReversedItems: _partialReversedItems.Pop(), 
                    partialRebuiltItems: _partialRebuiltItems.Push(_partialReversedItems.Peek()))
                // then, if there are new items, place them in _itemsTraverser for iterative transfer
              : _items.Count > _partialRebuiltItems.Count ? With(
                    itemsTraverser: _items.Pop().KeepOnly(_items.Count - _partialRebuiltItems.Count - 1), 
                    partialReversedItems: _partialReversedItems.Push(_items.Peek))
                // if there were no new items, then we've finished rebuilding and can use the result
              : new DropCollectStack<T>(_partialRebuiltItems, _partialRebuiltItems);
        }

        ///<summary>A modified copy with an item added to the front of the queue.</summary>
        public DropCollectStack<T> Push(T value) {
            return With(_items.Push(value));
        }

        ///<summary>A modified copy with the least-recently added item removed from the queue.</summary>
        public DropCollectStack<T> Drop() {
            if (Count == 0) throw new InvalidOperationException("Empty");
            if (Count == 1) return Empty;

            // drop from main items, noting the garbage
            var t = With(_items.Drop());

            // drop from iterative rebuild process
            var r =
                // in the partially rebuilt stack?
                !_partialRebuiltItems.IsEmpty ? t.With(partialRebuiltItems: _partialRebuiltItems.Drop())
                // in the non-rebuilt stack?
              : !_itemsTraverser.IsEmpty ? t.With(itemsTraverser: _itemsTraverser.Drop())
                // in the intermediate reversed stack?
              : !_partialReversedItems.IsEmpty ? t.With(partialReversedItems: _partialReversedItems.Pop())
                // I guess it's nowhere
              : t;

            // do iterative rebuilding fast enough to prevent garbage from accumulating
            return r.IterRebuild().IterRebuild().IterRebuild();
        }
    }
}
