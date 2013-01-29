using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Methods.ActorModel {
    ///<summary>Runs posted methods in order, without overlap, on some underlying synchronization context.</summary>
    public sealed class ActorSynchronizationContext : SynchronizationContext {
        private readonly SynchronizationContext _subContext;
        private readonly ConcurrentQueue<Action> _pending = new ConcurrentQueue<Action>();
        private int _pendingCount;

        ///<summary>Creates a new exclusive synchronization context, which runs callbacks on either an optional non-null context or else the thread pool.</summary>
        ///<param name="subContext">The synchronization context that actions will be run on. Defaults to the thread pool when null.</param>
        public ActorSynchronizationContext(SynchronizationContext subContext = null) {
            this._subContext = subContext ?? new SynchronizationContext();
        }

        public override void Post(SendOrPostCallback d, object state) {
            if (d == null) throw new ArgumentNullException("d");
            _pending.Enqueue(() => d(state));

            // trigger consumption when the queue was empty
            if (Interlocked.Increment(ref _pendingCount) == 1)
                _subContext.Post(Consume, null);
        }
        private void Consume(object state) {
            var surroundingContext = Current;
            try {
                SetSynchronizationContext(this); // temporarily replace surrounding sync context with this context

                // run pending actions until there are no more
                do {
                    Action a;
                    _pending.TryDequeue(out a); // always succeeds, due to usage of _pendingCount
                    a.Invoke(); // if an enqueued action throws... well, that's very bad
                } while (Interlocked.Decrement(ref _pendingCount) > 0); // stop consumption when something will think the queue was empty

            } finally {
                SetSynchronizationContext(surroundingContext); // restore surrounding sync context
            }
        }
        public override void Send(SendOrPostCallback d, object state) {
            throw new NotSupportedException();
        }
        public override SynchronizationContext CreateCopy() {
            return this;
        }
    }
}