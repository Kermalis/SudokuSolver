using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    class CellSnapshot
    {
        public int Value { get; }
        public ReadOnlyCollection<int> Candidates { get; }
        public bool IsCulprit { get; }

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
        public HashSet<int> Candidates { get; }
        public int OriginalValue { get; private set; }
        public int BlockIndex { get; }
        public SPoint Point { get; }
        public List<CellSnapshot> Snapshots { get; } = new List<CellSnapshot>();

        private readonly Puzzle _puzzle;
        public Cell(Puzzle puzzle, int value, SPoint point, HashSet<int> candidates = null)
        {
            _puzzle = puzzle;

            OriginalValue = Value = value;
            Point = point;
            BlockIndex = (point.X / 3) + (3 * (point.Y / 3));
            Candidates = candidates ?? new HashSet<int>(Utils.OneToNine);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newValue">value for this cell, 0 unset the value</param>
        /// <param name="refreshOtherCellCandidates"></param>
        public void Set(int newValue, bool refreshOtherCellCandidates = false)
        {
            Value = newValue;
            if (newValue == 0)
            {
                Unset();
            }
            else
            {
                Candidates.Clear();
                _puzzle.RemoveCandidates(GetCellsVisible(), new[] { newValue });
            }
            if (refreshOtherCellCandidates)
            {
                _puzzle.RefreshCandidates();
            }
        }
        /// <summary>
        /// unset the value for this cell, deletes all candidates
        /// </summary>
        public void Unset()
        {
            var oldValue = Value;
            Value = 0;
            foreach (int i in Utils.OneToNine)
            {
                Candidates.Add(i);
            }
            _puzzle.AddCandidates(GetCellsVisible(), new[] { oldValue });
        }
        public void ChangeOriginalValue(int value)
        {
            Set(OriginalValue = value, true);
        }
        public void AddSnapshot(bool isCulprit)
        {
            Snapshots.Add(new CellSnapshot(Value, Candidates, isCulprit));
        }

        /// <summary>
        /// clones a cell and it's candidates
        /// </summary>
        /// <returns></returns>
        public Cell Clone()
        {
            HashSet<int> candidates = new HashSet<int>();
            foreach (int candidate in Candidates)
            {
                candidates.Add(candidate);
            }
            return new Cell(_puzzle, Value, Point, candidates);
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
            return _puzzle.Columns[Point.X].Cells
                .Union(_puzzle.Rows[Point.Y].Cells)
                .Union(_puzzle.Blocks[BlockIndex].Cells)
                .Except(new Cell[] { this });
        }
    }
}
