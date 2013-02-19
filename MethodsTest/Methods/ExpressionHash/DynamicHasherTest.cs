using System.Linq;
using Methods.ExpressionHash;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DynamicHasherTest {
    [TestMethod]
    public void SumHash() {
        var sum = new DynamicHasher {
            InitialState = new[] {0, 0},
            Steps = new[] {new DynamicHasher.Step {LeftInputIndex = 1, RightInputIndex = 0, Operation = DynamicHasher.Operation.Add, OutputIndex = 1}}
        };

        sum.Interpret(new RepeatStream(1, 100)).AssertEquals(100);
        sum.Interpret(new CountingStream(100)).AssertEquals(Enumerable.Range(0, 100).Sum());
        var g = sum.Specialize().Compile();
        g(new RepeatStream(1, 100)).AssertEquals(100);
        g(new CountingStream(100)).AssertEquals(Enumerable.Range(0, 100).Sum());
    }
    [TestMethod]
    public void ExampleHash() {
        var r1 = ExpressionHashUtil.Example.Interpret(new CountingStream(100));
        var r2 = ExpressionHashUtil.SpecializedExampleFunction(new CountingStream(100));
        var r3 = ExpressionHashUtil.Example.Specialize().Compile()(new CountingStream(100));
        r1.AssertEquals(r2);
        r1.AssertEquals(r3);

        var s1 = ExpressionHashUtil.Example.Interpret(new RepeatStream(50, 100));
        var s2 = ExpressionHashUtil.SpecializedExampleFunction(new RepeatStream(50, 100));
        var s3 = ExpressionHashUtil.Example.Specialize().Compile()(new RepeatStream(50, 100));
        s1.AssertEquals(s2);
        s1.AssertEquals(s3);
    }
}
