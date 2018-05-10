using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Core
{
    public class Snapshot
    {
        public readonly int Value;
        public readonly int[] Candidates;
        public readonly bool IsCulprit;

        public Snapshot(int value, HashSet<int> candidates, bool culprit)
        {
            Value = value;
            Candidates = candidates.ToArray();
            IsCulprit = culprit;
        }
    }

    public class Cell
    {
        public int Value { get; private set; }
        public HashSet<int> Candidates { get; private set; }

        public int OriginalValue { get; private set; }
        public readonly int Block;
        public readonly SPoint Point;

        List<Snapshot> _snapshots;
        public Snapshot[] Snapshots { get => _snapshots.ToArray(); }

        Puzzle puzzle;

        public Cell(Puzzle board, int value, SPoint point)
        {
            puzzle = board;
            OriginalValue = Value = value;
            Point = point;
            Block = (point.X / 3) + (3 * (point.Y / 3));
            Candidates = new HashSet<int>(Enumerable.Range(1, 9));
            _snapshots = new List<Snapshot>();
        }

        public void Set(int newVal, bool refreshOthers = false)
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
        public void ChangeOriginal(int value) => Set(OriginalValue = value, true);
        public void TakeSnapshot(bool culprit) => _snapshots.Add(new Snapshot(Value, Candidates, culprit));

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
