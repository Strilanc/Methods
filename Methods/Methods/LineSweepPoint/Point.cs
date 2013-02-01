namespace Methods.LineSweepPoint {
    ///<summary>A 2d point (stripped to only include used methods).</summary>
    public struct Point {
        public readonly double X;
        public readonly double Y;
        public Point(double x, double y) {
            this.X = x;
            this.Y = y;
        }
        public static Point operator +(Point point, Vector delta) {
            return new Point(point.X + delta.X, point.Y + delta.Y);
        }
        public static Vector operator -(Point value1, Point value2) {
            return new Vector(value1.X - value2.X, value1.Y - value2.Y);
        }
    }
}