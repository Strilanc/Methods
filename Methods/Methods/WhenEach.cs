using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace Methods {
    public static class WhenEachMethod {
        ///<summary>Forwards tasks from the underlying observable, after they've completed, potentially out of order.</summary>
        public static IObservable<T> WhenEach<T>(this IObservable<T> observable) where T : Task {
            if (observable == null) throw new ArgumentNullException("observable");
            return new AnonymousObservable<T>(observer => {
                if (observer == null) throw new ArgumentNullException("observer");

                // state for tracking pending tasks, to ensure completion is after the last item
                Action sendDone = observer.OnCompleted;
                var pendingCount = 1;
                Action markOnePendingCompleted = () => {
                    if (Interlocked.Decrement(ref pendingCount) == 0) 
                        sendDone();
                };

                return observable.Subscribe(
                    task => {
                        if (task == null) {
                            observer.OnNext(null);
                            return;
                        }
                        Interlocked.Increment(ref pendingCount);
                        task.ContinueWith(x => observer.OnNext(task), TaskContinuationOptions.ExecuteSynchronously)
                            .ContinueWith(x => markOnePendingCompleted(), TaskContinuationOptions.ExecuteSynchronously);
                    },
                    ex => {
                        sendDone = () => observer.OnError(ex);
                        markOnePendingCompleted();
                    },
                    markOnePendingCompleted);
            });
        }
    }
}
