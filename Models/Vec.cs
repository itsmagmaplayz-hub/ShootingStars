namespace ShootingStars.Models
{
    public class Vec
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vec(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Vec other)
        {
            return System.Math.Sqrt(System.Math.Pow(X - other.X, 2) + System.Math.Pow(Y - other.Y, 2));
        }

        public override string ToString()
        {
            return $"({X:0.00}, {Y:0.00})";
        }
    }
}
