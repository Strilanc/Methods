using System;
using System.Threading;
using System.Threading.Tasks;
using Methods.ActorModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SynchronizationContextAwaiterTest {
    [TestMethod]
    public void CanAwait() {
        new Func<Task>(async () => {
            await new SynchronizationContext();
            throw new InvalidOperationException("test");
        }).Invoke().AssertFailed<InvalidOperationException>();

        new Func<Task<int>>(async () => {
            await new SynchronizationContext();
            return 5;
        }).Invoke().AssertRanToCompletion().AssertEquals(5);
    }
}
