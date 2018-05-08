using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SudokuSolver.Core
{
    public class Cell
    {
        public int Value { get; private set; }
        public readonly int OriginalValue;
        public readonly int Block;
        public readonly HashSet<int> Candidates;
        public readonly Point Coordinate;

        Board _board;

        public Cell(Board board, int value, Point point)
        {
            _board = board;
            OriginalValue = Value = value;
            Coordinate = point;
            Block = (point.X / 3) + (3 * (point.Y / 3));
            Candidates = new HashSet<int>(Enumerable.Range(1, 9));
        }
        
        public void Set(int value)
        {
            Value = value;
            Candidates.Clear();
            _board.BlacklistCandidates(GetCanSeePoints(), new int[] { value });
        }

        public override bool Equals(object obj)
        {
            if (obj is Cell other)
                return other.Value == Value && other.Coordinate == Coordinate;
            return false;
        }
        public override string ToString() => Value.ToString();

        public static implicit operator int(Cell c) => c.Value;

        public static bool operator ==(Cell lhs, int rhs) => lhs.Value == rhs;
        public static bool operator !=(Cell lhs, int rhs) => lhs.Value != rhs;
        public static bool operator ==(Cell lhs, Cell rhs) => lhs.Equals(rhs);
        public static bool operator !=(Cell lhs, Cell rhs) => !lhs.Equals(rhs);

        // Returns other cells the input cell can see
        internal Cell[] GetCanSee() => Board.Columns[Coordinate.X].Cells.Union(Board.Rows[Coordinate.Y].Cells).Union(Board.Blocks[Block].Cells).Except(new Cell[] { this }).ToArray();
        internal Point[] GetCanSeePoints() => GetCanSee().Select(c => c.Coordinate).ToArray();
    }
}
