namespace Methods.LineSweepPoint {
    ///<summary>A 2d vector (stripped to only include used methods).</summary>
    public struct Vector {
        public readonly double X;
        public readonly double Y;
        public Vector(double x, double y) {
            this.X = x;
            this.Y = y;
        }
        ///<summary>Difference.</summary>
        public static Vector operator -(Vector value1, Vector value2) {
            return new Vector(value1.X - value2.X, value1.Y - value2.Y);
        }
        ///<summary>Negate.</summary>
        public static Vector operator -(Vector value) {
            return new Vector(-value.X, -value.Y);
        }
        ///<summary>Scale.</summary>
        public static Vector operator *(Vector value, double scale) {
            return new Vector(value.X * scale, value.Y * scale);
        }
        ///<summary>Dot product.</summary>
        public static double operator *(Vector value1, Vector value2) {
            return value1.X * value2.X + value1.Y * value2.Y;
        }
        ///<summary>Cross product.</summary>
        public double Cross(Vector other) {
            return X * other.Y - Y * other.X;
        }
    }
}