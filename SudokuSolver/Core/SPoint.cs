namespace SudokuSolver.Core
{
    public class SPoint
    {
        public readonly int X, Y;

        public SPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator SPoint(System.Drawing.Point p) => new SPoint(p.X, p.Y);

        public override bool Equals(object obj)
        {
            if (obj is SPoint other)
                return other.X == X && other.Y == Y;
            return false;
        }
        public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode();
        public static string RowL(int r) => ((char)(r + 65)).ToString();
        public override string ToString() => RowL(Y) + (X + 1).ToString();
    }
}
