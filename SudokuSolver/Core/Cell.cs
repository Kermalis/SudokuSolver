using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Core
{
    internal class Snapshot
    {
        internal readonly int Value;
        internal readonly int[] Candidates;
        internal readonly bool IsCulprit;

        internal Snapshot(int value, HashSet<int> candidates, bool culprit)
        {
            Value = value;
            Candidates = candidates.ToArray();
            IsCulprit = culprit;
        }
    }

    internal class Cell
    {
        internal int Value { get; private set; }
        internal HashSet<int> Candidates { get; private set; }

        internal int OriginalValue { get; private set; }
        internal readonly int Block;
        internal readonly SPoint Point;

        List<Snapshot> _snapshots;
        internal Snapshot[] Snapshots { get => _snapshots.ToArray(); }

        Puzzle puzzle;

        internal Cell(Puzzle board, int value, SPoint point)
        {
            puzzle = board;
            OriginalValue = Value = value;
            Point = point;
            Block = (point.X / 3) + (3 * (point.Y / 3));
            Candidates = new HashSet<int>(Enumerable.Range(1, 9));
            _snapshots = new List<Snapshot>();
        }

        internal void Set(int newVal, bool refreshOthers = false)
        {
            int oldVal = Value;
            Value = newVal;
            if (newVal == 0)
            {
                Candidates = new HashSet<int>(Enumerable.Range(1, 9));
                puzzle.ChangeCandidates(GetCanSeePoints(), new int[] { oldVal }, false);
            }
            else
            {
                Candidates.Clear();
                puzzle.ChangeCandidates(GetCanSeePoints(), new int[] { newVal });
            }
            if (refreshOthers) puzzle.RefreshCandidates();
        }
        internal void ChangeOriginal(int value) => Set(OriginalValue = value, true);
        internal void TakeSnapshot(bool culprit) => _snapshots.Add(new Snapshot(Value, Candidates, culprit));

        public override int GetHashCode() => Point.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is Cell other)
                return other.Value == Value && other.Point.Equals(Point);
            return false;
        }
        public override string ToString()
        {
            string s = Point.ToString() + " ";
            if (Value == 0)
                s += "has candidates: " + Candidates.Print();
            else
                s += "- " + Value.ToString();
            return s;
        }

        public static implicit operator int(Cell c) => c.Value;

        public static bool operator ==(Cell lhs, int rhs) => lhs.Value == rhs;
        public static bool operator !=(Cell lhs, int rhs) => lhs.Value != rhs;
        public static bool operator ==(Cell lhs, Cell rhs) => lhs.Equals(rhs);
        public static bool operator !=(Cell lhs, Cell rhs) => !lhs.Equals(rhs);

        // Returns other cells the input cell can see
        internal Cell[] GetCanSee() => Puzzle.Columns[Point.X].Cells.Union(Puzzle.Rows[Point.Y].Cells).Union(Puzzle.Blocks[Block].Cells).Except(new Cell[] { this }).ToArray();
        internal SPoint[] GetCanSeePoints() => GetCanSee().Select(c => c.Point).ToArray();
    }
}
