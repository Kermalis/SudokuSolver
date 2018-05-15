using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SudokuSolver.Core
{
    internal class Puzzle
    {
        internal static Region[] Rows { get; private set; }
        internal static Region[] Columns { get; private set; }
        internal static Region[] Blocks { get; private set; }
        internal static Region[][] Regions { get; private set; }

        internal readonly bool IsCustom;

        Cell[][] board;

        internal Puzzle(int[][] inBoard, bool bCustom)
        {
            board = Utils.CreateJaggedArray<Cell[][]>(9, 9);
            IsCustom = bCustom;
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    board[x][y] = new Cell(this, inBoard[x][y], new SPoint(x, y));
            Regions = new Region[][] { Rows = new Region[9], Columns = new Region[9], Blocks = new Region[9] };
            for (int i = 0; i < 9; i++)
            {
                Rows[i] = new Region(this, SudokuRegion.Row, i);
                Columns[i] = new Region(this, SudokuRegion.Column, i);
                Blocks[i] = new Region(this, SudokuRegion.Block, i);
            }
        }

        internal Cell this[int x, int y]
        {
            get => board[x][y];
        }
        internal Cell this[SPoint p]
        {
            get => board[p.X][p.Y];
        }

        // Add/Remove the following candidates at the following locations
        internal bool ChangeCandidates(IEnumerable<Cell> cells, IEnumerable<int> cand, bool remove = true) => ChangeCandidates(cells.Select(c => c.Point), cand, remove);
        internal bool ChangeCandidates(IEnumerable<SPoint> points, IEnumerable<int> cand, bool remove = true)
        {
            bool changed = false;
            foreach (SPoint p in points)
                foreach (int v in cand)
                    if (remove ? this[p].Candidates.Remove(v) : this[p].Candidates.Add(v)) changed = true;
            return changed;
        }
        internal void RefreshCandidates()
        {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    foreach (var i in Enumerable.Range(1, 9).Except(this[x, y].Candidates))
                        this[x, y].Candidates.Add(i);
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    if (this[x, y] != 0) this[x, y].Set(this[x, y]);
        }

        internal static bool Load(string fileName, out Solver solver)
        {
            solver = null;
            string[] filelines = File.ReadAllLines(fileName);
            if (filelines.Length != 9) return false;
            var board = Utils.CreateJaggedArray<int[][]>(9, 9);
            for (int i = 0; i < 9; i++)
            {
                string line = filelines[i];
                if (line.Length != 9) return false;
                for (int j = 0; j < 9; j++)
                    if (byte.TryParse(line[j].ToString(), out byte value)) // Anything can represent 0
                        board[j][i] = value;
            }

            solver = new Solver(board, false);
            return true;
        }
        internal void Save(string fileName)
        {
            using (var file = new StreamWriter(fileName))
                for (int x = 0; x < 9; x++)
                {
                    string line = "";
                    for (int y = 0; y < 9; y++)
                        line += this[y, x].OriginalValue == 0 ? "-" : this[y, x].OriginalValue.ToString();
                    file.WriteLine(line);
                }
        }
    }
}
