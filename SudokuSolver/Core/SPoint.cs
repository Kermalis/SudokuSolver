namespace Kermalis.SudokuSolver.Core
{
    class SPoint
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
            {
                return other.X == X && other.Y == Y;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return unchecked(X ^ Y);
        }
        public static string RowLetter(int row)
        {
            return ((char)(row + 65)).ToString();
        }
        public static string ColumnLetter(int column)
        {
            return (column + 1).ToString();
        }
        public override string ToString()
        {
            return RowLetter(Y) + ColumnLetter(X);
        }
    }
}
