using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Methods {
    static class Program {
        static void Main(string[] args) {
            WhenEachExample();
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
    }
}
