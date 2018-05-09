using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SudokuSolver.Core
{
    public class Board
    {
        public static Region[] Rows { get; private set; }
        public static Region[] Columns { get; private set; }
        public static Region[] Blocks { get; private set; }
        public static Region[][] Regions { get; private set; }

        public readonly bool IsCustom;

        Cell[][] _board;
        public readonly BindingList<string> Actions;

        public Board(int[][] inBoard, bool bCustom)
        {
            _board = Utils.CreateJaggedArray<Cell[][]>(9, 9);
            IsCustom = bCustom;
            Actions = new BindingList<string>();
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    _board[x][y] = new Cell(this, inBoard[x][y], new SPoint(x, y));
            Regions = new Region[][] { Rows = new Region[9], Columns = new Region[9], Blocks = new Region[9] };
            for (int i = 0; i < 9; i++)
            {
                Rows[i] = new Region(this, SudokuRegion.Row, i);
                Columns[i] = new Region(this, SudokuRegion.Column, i);
                Blocks[i] = new Region(this, SudokuRegion.Block, i);
            }
        }

        public Cell this[int x, int y]
        {
            get => _board[x][y];
        }
        public Cell this[SPoint p]
        {
            get => _board[p.X][p.Y];
        }

        public void Log(string technique, IEnumerable<SPoint> culprits, string format, params object[] args) => Log(technique, culprits.Select(p => this[p]), format, args);
        public void Log(string technique, IEnumerable<Cell> culprits, string format, params object[] args) => Log(string.Format($"{technique,-25}" + format, args), culprits);
        public void Log(string s, IEnumerable<Cell> culprits = null)
        {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    this[x, y].TakeSnapshot(culprits != null && culprits.Contains(this[x, y]));
            Actions.Add(s);
        }

        // Add/Remove the following candidates at the following locations
        public bool ChangeCandidates(IEnumerable<SPoint> points, IEnumerable<int> cand, bool remove = true)
        {
            bool changed = false;
            foreach (SPoint p in points)
                foreach (int v in cand)
                    if (remove ? this[p].Candidates.Remove(v) : this[p].Candidates.Add(v)) changed = true;
            return changed;
        }
        public void RefreshCandidates()
        {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    if (this[x, y] != 0) this[x, y].Set(this[x, y]);
        }
    }
}
