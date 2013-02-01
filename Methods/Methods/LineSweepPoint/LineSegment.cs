namespace Methods.LineSweepPoint {
    ///<summary>A 2d line segment (stripped to only include used methods).</summary>
    public struct LineSegment {
        public readonly Point Start;
        public readonly Vector Delta;
        public Point End { get { return Start + Delta; } }
        public LineSegment(Point start, Point end) {
            this.Start = start;
            this.Delta = end - start;
        }
        public Point LerpAcross(double proportion) {
            return Start + Delta * proportion;
        }
    }
}