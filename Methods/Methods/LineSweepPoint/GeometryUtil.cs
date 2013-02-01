using System;
using System.Collections.Generic;
using System.Linq;

namespace Methods.LineSweepPoint {
    public static class GeometryUtil {
        /// <summary>
        /// Enumerates the times and places, if any, where a moving line crosses a point during a time step.
        /// The line's endpoints move at a constant rate along their linear paths.
        /// </summary>
        public static IEnumerable<Sweep> WhenLineSweepsPoint(LineSegment pathOfLineStartPoint,
                                                             LineSegment pathOfLineEndPoint,
                                                             Point point) {
            var a = point - pathOfLineStartPoint.Start;
            var b = -pathOfLineStartPoint.Delta;
            var c = pathOfLineEndPoint.Start - pathOfLineStartPoint.Start;
            var d = pathOfLineEndPoint.Delta - pathOfLineStartPoint.Delta;

            return from t in QuadraticRoots(b.Cross(d), a.Cross(d) + b.Cross(c), a.Cross(c))
                   where t >= 0 && t <= 1
                   let start = pathOfLineStartPoint.LerpAcross(t)
                   let end = pathOfLineEndPoint.LerpAcross(t)
                   let s = point.LerpProjectOnto(new LineSegment(start, end))
                   where s >= 0 && s <= 1
                   orderby t
                   select new Sweep(timeProportion: t, acrossProportion: s);
        }

        public struct Sweep {
            public readonly double TimeProportion;
            public readonly double AcrossProportion;
            public Sweep(double timeProportion, double acrossProportion) {
                this.TimeProportion = timeProportion;
                this.AcrossProportion = acrossProportion;
            }
        }

        ///<summary>
        ///The proportion that, when lerped across the given line, results in the given point.
        ///If the point is not on the line segment, the result is the closest point on the extended line.
        ///</summary>
        public static double LerpProjectOnto(this Point point, LineSegment line) {
            var b = point - line.Start;
            var d = line.Delta;
            return (b * d) / (d * d);
        }

        ///<summary>
        ///Enumerates the real solutions to the formula a*x^2 + b*x + c = 0.
        ///Handles degenerate cases.
        ///If a=b=c=0 then only zero is enumerated, even though technically all real numbers are solutions.
        ///</summary>
        public static IEnumerable<double> QuadraticRoots(double a, double b, double c) {
            // degenerate? (0x^2 + bx + c == 0)
            if (a == 0) {
                // double-degenerate? (0x^2 + 0x + c == 0)
                if (b == 0) {
                    // triple-degenerate? (0x^2 + 0x + 0 == 0)
                    if (c == 0) {
                        // every other real number is also a solution, but hopefully one example will be fine
                        yield return 0;
                    }
                    yield break;
                }

                yield return -c / b;
                yield break;
            }

            // ax^2 + bx + c == 0
            // x = (-b +- sqrt(b^2 - 4ac)) / 2a

            var d = b * b - 4 * a * c;
            if (d < 0) yield break; // no real roots

            var s0 = -b / (2 * a);
            var sd = Math.Sqrt(d) / (2 * a);
            yield return s0 - sd;
            if (sd == 0) yield break; // unique root

            yield return s0 + sd;
        }
    }
}
