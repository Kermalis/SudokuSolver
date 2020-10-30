using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    internal sealed class CellSnapshot
    {
        public int Value { get; }
        public ReadOnlyCollection<int> Candidates { get; }
        public bool IsCulprit { get; }
        public bool IsSemiCulprit { get; }

        public CellSnapshot(int value, HashSet<int> candidates, bool isCulprit, bool isSemiCulprit)
        {
            Value = value;
            Candidates = new ReadOnlyCollection<int>(candidates.ToArray());
            IsCulprit = isCulprit;
            IsSemiCulprit = isSemiCulprit;
        }
    }

    [DebuggerDisplay("{DebugString()}", Name = "{ToString()}")]
    internal sealed class Cell
    {
        public int Value { get; private set; }
        public HashSet<int> Candidates { get; } = new HashSet<int>(Utils.OneToNine);

        public int OriginalValue { get; private set; }
        public SPoint Point { get; }

        public List<CellSnapshot> Snapshots { get; } = new List<CellSnapshot>();
        private readonly Puzzle _puzzle;

        public Cell(Puzzle puzzle, int value, SPoint point)
        {
            _puzzle = puzzle;
            OriginalValue = Value = value;
            Point = point;
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
                _puzzle.ChangeCandidates(GetCellsVisible(), oldValue, remove: false);
            }
            else
            {
                Candidates.Clear();
                _puzzle.ChangeCandidates(GetCellsVisible(), newValue);
            }
            if (refreshOtherCellCandidates)
            {
                _puzzle.RefreshCandidates();
            }
        }
        public void ChangeOriginalValue(int value)
        {
            OriginalValue = value;
            Set(value, refreshOtherCellCandidates: true);
        }
        public void CreateSnapshot(bool isCulprit, bool isSemiCulprit)
        {
            Snapshots.Add(new CellSnapshot(Value, Candidates, isCulprit, isSemiCulprit));
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

        /// <summary>Returns other cells the input cell can "see" (besides the input cell)</summary>
        public IEnumerable<Cell> GetCellsVisible()
        {
            Region block = _puzzle.Blocks[Point.BlockIndex];
            Region col = _puzzle.Columns[Point.X];
            Region row = _puzzle.Rows[Point.Y];
            return block.Union(col).Union(row).Except(new Cell[] { this });
        }
    }
}
