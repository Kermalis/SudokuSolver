using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Core
{
    public class Cell
    {
        public int Value { get; private set; }
        public readonly int OriginalValue;
        public readonly int Block;
        public readonly HashSet<int> Candidates;
        public readonly SPoint Point;

        Board _board;

        public Cell(Board board, int value, SPoint point)
        {
            _board = board;
            OriginalValue = Value = value;
            Point = point;
            Block = (point.X / 3) + (3 * (point.Y / 3));
            Candidates = new HashSet<int>(Enumerable.Range(1, 9));
        }

        public void Set(int value)
        {
            Value = value;
            Candidates.Clear();
            _board.BlacklistCandidates(GetCanSeePoints(), new int[] { value });
        }

        public override int GetHashCode() => Point.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is Cell other)
                return other.Value == Value && other.Point.Equals(Point);
            return false;
        }
        public override string ToString() => Value.ToString();

        public static implicit operator int(Cell c) => c.Value;

        public static bool operator ==(Cell lhs, int rhs) => lhs.Value == rhs;
        public static bool operator !=(Cell lhs, int rhs) => lhs.Value != rhs;
        public static bool operator ==(Cell lhs, Cell rhs) => lhs.Equals(rhs);
        public static bool operator !=(Cell lhs, Cell rhs) => !lhs.Equals(rhs);

        // Returns other cells the input cell can see
        internal Cell[] GetCanSee() => Board.Columns[Point.X].Cells.Union(Board.Rows[Point.Y].Cells).Union(Board.Blocks[Block].Cells).Except(new Cell[] { this }).ToArray();
        internal SPoint[] GetCanSeePoints() => GetCanSee().Select(c => c.Point).ToArray();
    }
}
