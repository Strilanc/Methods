using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Methods.ActorModel {
    public sealed class SynchronizationContextAwaiter : INotifyCompletion {
        private readonly SynchronizationContext _context;
        public SynchronizationContextAwaiter(SynchronizationContext context) {
            if (context == null) throw new ArgumentNullException("context");
            _context = context;
        }
        public bool IsCompleted {
            get {
                // always re-enter, even if already in the context
                return false;
            }
        }
        public void OnCompleted(Action action) {
            _context.Post(x => action(), null);
        }
        public void GetResult() {}
    }
}