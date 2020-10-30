using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    internal sealed class Puzzle
    {
        public readonly ReadOnlyCollection<Region> Rows;
        public readonly ReadOnlyCollection<Region> Columns;
        public readonly ReadOnlyCollection<Region> Blocks;
        public readonly ReadOnlyCollection<ReadOnlyCollection<Region>> Regions;

        public readonly BindingList<string> Actions = new BindingList<string>();
        public readonly bool IsCustom;
        private readonly Cell[][] _board;

        public Cell this[int x, int y] => _board[x][y];

        public Puzzle(int[][] board, bool isCustom)
        {
            IsCustom = isCustom;
            _board = Utils.CreateJaggedArray<Cell[][]>(9, 9);
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    _board[x][y] = new Cell(this, board[x][y], new SPoint(x, y));
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
                    cells[c] = _board[c][i];
                }
                rows[i] = new Region(cells);

                for (c = 0; c < 9; c++)
                {
                    cells[c] = _board[i][c];
                }
                columns[i] = new Region(cells);

                c = 0;
                int ix = i % 3 * 3,
                    iy = i / 3 * 3;
                for (int x = ix; x < ix + 3; x++)
                {
                    for (int y = iy; y < iy + 3; y++)
                    {
                        cells[c++] = _board[x][y];
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

        public bool ChangeCandidates(Cell cell, int candidate, bool remove = true)
        {
            return remove ? cell.Candidates.Remove(candidate) : cell.Candidates.Add(candidate);
        }
        public bool ChangeCandidates(Cell cell, IEnumerable<int> candidates, bool remove = true)
        {
            bool changed = false;
            foreach (int value in candidates)
            {
                if (remove ? cell.Candidates.Remove(value) : cell.Candidates.Add(value))
                {
                    changed = true;
                }
            }
            return changed;
        }
        public bool ChangeCandidates(IEnumerable<Cell> cells, int candidate, bool remove = true)
        {
            bool changed = false;
            foreach (Cell cell in cells)
            {
                if (remove ? cell.Candidates.Remove(candidate) : cell.Candidates.Add(candidate))
                {
                    changed = true;
                }
            }
            return changed;
        }
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
                    string line = string.Empty;
                    for (int y = 0; y < 9; y++)
                    {
                        Cell cell = this[y, x];
                        if (cell.OriginalValue == 0)
                        {
                            line += '-';
                        }
                        else
                        {
                            line += cell.OriginalValue.ToString();
                        }
                    }
                    file.WriteLine(line);
                }
            }
        }

        public static string TechniqueFormat(string technique, string format, params object[] args)
        {
            return string.Format(string.Format("{0,-20}", technique) + format, args);
        }

        public void LogAction(string action)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(false, false);
                }
            }
            Actions.Add(action);
        }
        public void LogAction(string action, Cell culprit, Cell semiCulprit)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(culprit == cell, semiCulprit == cell);
                }
            }
            Actions.Add(action);
        }
        public void LogAction(string action, IEnumerable<Cell> culprits, Cell semiCulprit)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(culprits != null && culprits.Contains(cell), semiCulprit == cell);
                }
            }
            Actions.Add(action);
        }
        public void LogAction(string action, Cell culprit, IEnumerable<Cell> semiCulprits)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(culprit == cell, semiCulprits != null && semiCulprits.Contains(cell));
                }
            }
            Actions.Add(action);
        }
        public void LogAction(string action, IEnumerable<Cell> culprits, IEnumerable<Cell> semiCulprits)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    Cell cell = this[x, y];
                    cell.CreateSnapshot(culprits != null && culprits.Contains(cell), semiCulprits != null && semiCulprits.Contains(cell));
                }
            }
            Actions.Add(action);
        }
    }
}
