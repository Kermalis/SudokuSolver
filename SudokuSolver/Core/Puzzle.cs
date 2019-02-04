using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    class Puzzle
    {
        public readonly ReadOnlyCollection<Region> Rows;
        public readonly ReadOnlyCollection<Region> Columns;
        public readonly ReadOnlyCollection<Region> Blocks;
        public readonly ReadOnlyCollection<ReadOnlyCollection<Region>> Regions;

        public readonly BindingList<string> Actions = new BindingList<string>();
        public readonly bool IsCustom;
        readonly Cell[][] board;

        public Puzzle(int[][] board, bool isCustom)
        {
            IsCustom = isCustom;
            this.board = Utils.CreateJaggedArray<Cell[][]>(9, 9);
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    this.board[x][y] = new Cell(this, board[x][y], new SPoint(x, y));
                }
            }
            Region[] rows = new Region[9],
                columns = new Region[9],
                blocks = new Region[9];
            for (int i = 0; i < 9; i++)
            {
                Cell[] cells;
                int c;

                cells = new Cell[9];
                for (c = 0; c < 9; c++)
                {
                    cells[c] = this.board[c][i];
                }
                rows[i] = new Region(cells);

                cells = new Cell[9];
                for (c = 0; c < 9; c++)
                {
                    cells[c] = this.board[i][c];
                }
                columns[i] = new Region(cells);

                cells = new Cell[9];
                c = 0;
                int ix = i % 3 * 3,
                    iy = i / 3 * 3;
                for (int x = ix; x < ix + 3; x++)
                {
                    for (int y = iy; y < iy + 3; y++)
                    {
                        cells[c++] = this.board[x][y];
                    }
                }
                blocks[i] = new Region(cells);
            }
            Regions = new ReadOnlyCollection<ReadOnlyCollection<Region>>(new ReadOnlyCollection<Region>[]
            {
                Rows = new ReadOnlyCollection<Region>(rows),
                Columns = new ReadOnlyCollection<Region>(columns),
                Blocks = new ReadOnlyCollection<Region>(blocks)
            });
        }

        public Cell this[int x, int y]
        {
            get => board[x][y];
        }

        // Add/Remove the following candidates at the following locations
        public bool ChangeCandidates(IEnumerable<Cell> cells, IEnumerable<int> candidates, bool remove = true)
        {
            bool changed = false;
            foreach (Cell cell in cells)
            {
                foreach (int value in candidates)
                {
                    if (remove ? cell.Candidates.Remove(value) : cell.Candidates.Add(value))
                    {
                        changed = true;
                    }
                }
            }
            return changed;
        }
        public void RefreshCandidates()
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    foreach (int i in Utils.OneToNine)
                    {
                        cell.Candidates.Add(i);
                    }
                }
            }
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    if (cell.Value != 0)
                    {
                        cell.Set(cell.Value);
                    }
                }
            }
        }

        public static Puzzle Load(string fileName)
        {
            string[] fileLines = File.ReadAllLines(fileName);
            if (fileLines.Length != 9)
            {
                throw new InvalidDataException("Puzzle must have 9 rows.");
            }
            int[][] board = Utils.CreateJaggedArray<int[][]>(9, 9);
            for (int i = 0; i < 9; i++)
            {
                string line = fileLines[i];
                if (line.Length != 9)
                {
                    throw new InvalidDataException($"Row {i} must have 9 values.");
                }
                for (int j = 0; j < 9; j++)
                {
                    if (int.TryParse(line[j].ToString(), out int value)) // Anything can represent 0
                    {
                        board[j][i] = value;
                    }
                }
            }

            return new Puzzle(board, false);
        }
        public void Save(string fileName)
        {
            using (var file = new StreamWriter(fileName))
            {
                for (int x = 0; x < 9; x++)
                {
                    string line = "";
                    for (int y = 0; y < 9; y++)
                    {
                        Cell cell = this[y, x];
                        line += cell.OriginalValue == 0 ? "-" : cell.OriginalValue.ToString();
                    }
                    file.WriteLine(line);
                }
            }
        }

        public void LogAction(string technique, IEnumerable<Cell> culprits, IEnumerable<int> candidates)
        {
            LogAction(technique, culprits, "{0}: {1}", culprits.Count() == 1 ? culprits.ElementAt(0).ToString() : culprits.Print(), candidates.Count() == 1 ? candidates.ElementAt(0).ToString() : candidates.Print());
        }
        public void LogAction(string technique, IEnumerable<Cell> culprits, string format, params object[] args)
        {
            LogAction(string.Format(string.Format("{0,-20}", technique) + format, args), culprits);
        }
        public void LogAction(string action, IEnumerable<Cell> culprits = null)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.AddSnapshot(culprits != null && culprits.Contains(cell));
                }
            }
            Actions.Add(action);
        }
    }
}
