using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    class CellSnapshot
    {
        public readonly int Value;
        public readonly ReadOnlyCollection<int> Candidates;
        public readonly bool IsCulprit;

        public CellSnapshot(int value, HashSet<int> candidates, bool isCulprit)
        {
            Value = value;
            Candidates = new ReadOnlyCollection<int>(candidates.ToArray());
            IsCulprit = isCulprit;
        }
    }

    [DebuggerDisplay("{DebugString()}", Name = "{ToString()}")]
    class Cell
    {
        public int Value { get; private set; }
        public readonly HashSet<int> Candidates = new HashSet<int>(Utils.OneToNine);

        public int OriginalValue { get; private set; }
        public readonly int BlockIndex;
        public readonly SPoint Point;

        public readonly List<CellSnapshot> Snapshots = new List<CellSnapshot>();

        readonly Puzzle puzzle;

        public Cell(Puzzle puzzle, int value, SPoint point)
        {
            this.puzzle = puzzle;
            OriginalValue = Value = value;
            Point = point;
            BlockIndex = (point.X / 3) + (3 * (point.Y / 3));
        }

        public void Set(int newValue, bool refreshOtherCellCandidates = false)
        {
            int oldValue = Value;
            Value = newValue;
            if (newValue == 0)
            {
                foreach (int i in Utils.OneToNine)
                {
                    Candidates.Add(i);
                }
                puzzle.ChangeCandidates(GetCellsVisible(), new[] { oldValue }, false);
            }
            else
            {
                Candidates.Clear();
                puzzle.ChangeCandidates(GetCellsVisible(), new[] { newValue });
            }
            if (refreshOtherCellCandidates)
            {
                puzzle.RefreshCandidates();
            }
        }
        public void ChangeOriginalValue(int value)
        {
            Set(OriginalValue = value, true);
        }
        public void AddSnapshot(bool isCulprit)
        {
            Snapshots.Add(new CellSnapshot(Value, Candidates, isCulprit));
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Cell other)
            {
                return other.Point.Equals(Point);
            }
            return false;
        }
        public override string ToString()
        {
            return Point.ToString();
        }
        public string DebugString()
        {
            string s = Point.ToString() + " ";
            if (Value == 0)
            {
                s += "has candidates: " + Candidates.Print();
            }
            else
            {
                s += "- " + Value.ToString();
            }
            return s;
        }

        // Returns other cells the input cell can see
        public IEnumerable<Cell> GetCellsVisible()
        {
            return puzzle.Columns[Point.X].Cells.Union(puzzle.Rows[Point.Y].Cells).Union(puzzle.Blocks[BlockIndex].Cells).Except(new Cell[] { this });
        }
    }
}
