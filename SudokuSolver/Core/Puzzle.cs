using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SudokuSolver.Core
{
    public class Puzzle
    {
        public static Region[] Rows { get; private set; }
        public static Region[] Columns { get; private set; }
        public static Region[] Blocks { get; private set; }
        public static Region[][] Regions { get; private set; }

        public readonly bool IsCustom;

        Cell[][] board;
        public readonly BindingList<string> Actions;

        public Puzzle(int[][] inBoard, bool bCustom)
        {
            board = Utils.CreateJaggedArray<Cell[][]>(9, 9);
            IsCustom = bCustom;
            Actions = new BindingList<string>();
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

        public Cell this[int x, int y]
        {
            get => board[x][y];
        }
        public Cell this[SPoint p]
        {
            get => board[p.X][p.Y];
        }

        public void Log(string technique, IEnumerable<SPoint> culprits, params int[] candidates) => Log(technique, culprits, "{0}: {1}", culprits.Count() == 1 ? culprits.ElementAt(0).ToString() : culprits.Print(), candidates.Length == 1 ? candidates[0].ToString() : candidates.Print());
        public void Log(string technique, IEnumerable<Cell> culprits, params int[] candidates) => Log(technique, culprits.Select(c => c.Point), candidates);
        public void Log(string technique, IEnumerable<SPoint> culprits, string format, params object[] args) => Log(technique, culprits.Select(p => this[p]), format, args);
        public void Log(string technique, IEnumerable<Cell> culprits, string format, params object[] args) => Log(string.Format($"{technique,-20}" + format, args), culprits.ToArray());
        public void Log(string s, params Cell[] culprits)
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
                    foreach (var i in Enumerable.Range(1, 9).Except(this[x, y].Candidates))
                        this[x, y].Candidates.Add(i);
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    if (this[x, y] != 0) this[x, y].Set(this[x, y]);
        }

        public static bool Load(string fileName, out Solver solver)
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
        public void Save(string fileName)
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
