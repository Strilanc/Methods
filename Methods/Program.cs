using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Methods.ExpressionHash;

namespace Methods {
    static class Program {
        static void Main() {
            ExpressionHashTiming();
            Console.Read();
        }
        static async Task WhenEachExample() {
            var dt = 0.1; //s
            var skew = 1.0; //s
            var r = new Random();
            await Enumerable.Range(0, 100)
                .Select(e => Task.Delay(TimeSpan.FromSeconds(e * dt + r.NextDouble() * skew))
                                 .ContinueWith(x => e))
                .ToObservable()
                .WhenEach()
                .Where(e => e.IsCompleted)
                .Select(e => e.Result)
                .ForEachAsync(Console.WriteLine);
        }
        static void ExpressionHashTiming() {
            var timer = new Stopwatch();

            Console.WriteLine("Generating dynamically specialized code...");
            timer.Start();
            var specializedExpression = ExpressionHashUtil.Example.Specialize();
            var specializedMethod = specializedExpression.Compile();
            Console.WriteLine("Done after {0:0}ms", timer.Elapsed.TotalMilliseconds);

            // -- uncomment to output the IL to disk, where it can be inspected more easily
            //ExpressionHashUtil.WriteMethodToAssembly(specializedExpression, "Example");
            var interpretedDuration = TimeSpan.Zero;
            var specializedDuration = TimeSpan.Zero;
            
            Console.WriteLine("Timing interpretation vs dynamically generated code of example hash...");
            var repeatCount = 10;
            var streamSize = 1 << 24;
            foreach (var _ in Enumerable.Range(0, repeatCount)) {
                timer.Restart();
                var r1 = ExpressionHashUtil.Example.Interpret(new CountingStream(streamSize));
                var t = timer.Elapsed;
                interpretedDuration += t;
                Console.Write("Interpreted: {0:0}ms", t.TotalMilliseconds);
                
                timer.Restart();
                var r2 = specializedMethod(new CountingStream(streamSize));
                t = timer.Elapsed;
                specializedDuration += t;
                Console.Write(", Specialized: {0:0}ms", t.TotalMilliseconds);
                Console.WriteLine(r1 == r2 ? "" : ", !!!Didn't match!!!");
            }
        }
    }
}
