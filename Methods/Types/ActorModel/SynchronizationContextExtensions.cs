using System;
using System.Threading;

namespace Methods.ActorModel {
    public static class SynchronizationContextExtensions {
        /// <summary>
        /// Resumes execution in the given synchronization context, when awaited.
        /// If execution is already in the given context, it will re-enter it.
        /// </summary>
        public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context) {
            if (context == null) throw new ArgumentNullException("context");
            return new SynchronizationContextAwaiter(context);
        }
    }
}