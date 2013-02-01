using System.Collections.Generic;
using Methods.LineSweepPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

internal static class GeoTestUtil {
    public static LineSegment Sweep(this Point start, Vector delta) {
        return new LineSegment(start, start + delta);
    }
    public static LineSegment To(this Point start, Point end) {
        return new LineSegment(start, end);
    }
    public static void AssertSequenceApproximates(this IEnumerable<GeometryUtil.Sweep> actual, params GeometryUtil.Sweep[] expected) {
        var r1 = actual.ToArray();
        var r2 = expected.ToArray();
        r1.Length.AssertEquals(r2.Length);
        for (var i = 0; i < r1.Length; i++) {
            Assert.AreEqual(r1[i].TimeProportion, r2[i].TimeProportion, 0.0001);
            Assert.AreEqual(r1[i].AcrossProportion, r2[i].AcrossProportion, 0.0001);
        }
    }
    public static void AssertSequenceApproximates(this IEnumerable<double> actual, params double[] expected) {
        var r1 = actual.ToArray();
        var r2 = expected.ToArray();
        r1.Length.AssertEquals(r2.Length);
        for (var i = 0; i < r1.Length; i++) {
            Assert.AreEqual(r1[i], r2[i], 0.0001);
        }
    }
}

[TestClass]
public class GeometryUtilTest {
    [TestMethod]
    public void Cross() {
        new Vector(0, 1).Cross(new Vector(0, 1)).AssertEquals(0);
        new Vector(1, 0).Cross(new Vector(0, 1)).AssertEquals(1);
        new Vector(0, 1).Cross(new Vector(1, 0)).AssertEquals(-1);
        new Vector(0, 1).Cross(new Vector(2, 0)).AssertEquals(-2);
    }

    [TestMethod]
    public void LineSegmentProperties() {
        var line = new LineSegment(new Point(0, 1), new Point(1, 1));
        line.AssertEquals(new LineSegment(new Point(0, 1), new Point(1, 1)));
        line.Start.AssertEquals(new Point(0, 1));
        line.End.AssertEquals(new Point(1, 1));
        line.Delta.AssertEquals(new Vector(1, 0));
    }

    [TestMethod]
    public void WhenLineSweepsPoint() {
        // parallel
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, -1).Sweep(new Vector(0, 2)),
            new Point(+1, -1).Sweep(new Vector(0, 2)),
            default(Point)
        ).AssertSequenceApproximates(new GeometryUtil.Sweep(0.5, 0.5));
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, -1).Sweep(new Vector(2, 0)),
            new Point(-1, +1).Sweep(new Vector(2, 0)),
            default(Point)
        ).AssertSequenceApproximates(new GeometryUtil.Sweep(0.5, 0.5));
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, -1).Sweep(new Vector(0.5, 0)),
            new Point(-1, +1).Sweep(new Vector(0.5, 0)),
            default(Point)
        ).AssertSequenceApproximates();

        // fixed
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, 0).Sweep(new Vector(0, 0)),
            new Point(+1, 0).Sweep(new Vector(0, 0)),
            default(Point)
        ).AssertSequenceApproximates(new GeometryUtil.Sweep(0, 0.5));
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, -1).Sweep(new Vector(0, 0)),
            new Point(+1, -1).Sweep(new Vector(0, 0)),
            default(Point)
        ).AssertSequenceApproximates();

        // orthogonal
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, +10).Sweep(new Vector(0, -11)),
            new Point(-1, -1).Sweep(new Vector(11, 0)),
            default(Point)
        ).Count().AssertEquals(2);
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, +3).Sweep(new Vector(0, -4)),
            new Point(-1, -1).Sweep(new Vector(4, 0)),
            default(Point)
        ).AssertSequenceApproximates(new GeometryUtil.Sweep(0.5, 0.5));
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, +1).Sweep(new Vector(0, -2)),
            new Point(-1, -1).Sweep(new Vector(2, 0)),
            default(Point)
        ).AssertSequenceApproximates();

        // anchored
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, 0).Sweep(new Vector(0, 0)),
            new Point(+1, -1).To(new Point(+1, +1)),
            default(Point)
        ).AssertSequenceApproximates(new GeometryUtil.Sweep(0.5, 0.5));
        GeometryUtil.WhenLineSweepsPoint(
            new Point(-1, 0).Sweep(new Vector(0, 0)),
            new Point(+1, -1).To(new Point(+1, -0.5)),
            default(Point)
        ).AssertSequenceApproximates();
    }
    [TestMethod]
    public void Quadratic() {
        GeometryUtil.QuadraticRoots(1, 0, 0).AssertSequenceApproximates(0);
        GeometryUtil.QuadraticRoots(0, 1, 0).AssertSequenceApproximates(0);
        GeometryUtil.QuadraticRoots(0, 0, 1).AssertSequenceApproximates();
        
        GeometryUtil.QuadraticRoots(1, 1, 0).AssertSequenceApproximates(-1, 0);
        GeometryUtil.QuadraticRoots(0, 1, 1).AssertSequenceApproximates(-1);
        GeometryUtil.QuadraticRoots(1, 0, 1).AssertSequenceApproximates();

        GeometryUtil.QuadraticRoots(1, 0, -1).AssertSequenceApproximates(-1, 1);
        GeometryUtil.QuadraticRoots(1, 2, 1).AssertSequenceApproximates(-1);
        
        GeometryUtil.QuadraticRoots(0, 0, 0).Any().AssertIsTrue();
    }
}
