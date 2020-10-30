namespace Kermalis.SudokuSolver.Core
{
    internal sealed class SPoint
    {
        public int X { get; }
        public int Y { get; }
        public int BlockIndex { get; }

        public SPoint(int x, int y)
        {
            X = x;
            Y = y;
            BlockIndex = (x / 3) + (3 * (y / 3));
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
            return ((char)(row + 'A')).ToString();
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
