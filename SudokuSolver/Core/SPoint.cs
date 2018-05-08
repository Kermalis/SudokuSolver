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

        public override bool Equals(object obj)
        {
            if (obj is SPoint other)
                return other.X == X && other.Y == Y;
            return false;
        }
        public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode();
        public override string ToString() => ((char)(Y + 65)).ToString() + (X + 1).ToString();
    }
}
