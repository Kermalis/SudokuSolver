namespace Kermalis.SudokuSolver.Core
{
    class SPoint
    {
        public int X { get; }
        public int Y { get; }

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
            //better way of calcalating hash according to
            /// https://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers
            var hash = 23;
            hash = hash * 31 + X;
            hash = hash * 31 + Y;
            return hash;
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
