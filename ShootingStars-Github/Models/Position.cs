namespace ShootingStars.Models
{
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Position other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public override bool Equals(object? obj)
        {
            if (obj is Position pos)
            {
                return X == pos.X && Y == pos.Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
