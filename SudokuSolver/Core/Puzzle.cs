using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    class Puzzle
    {
        public ReadOnlyCollection<Region> Rows { get; }
        public ReadOnlyCollection<Region> Columns { get; }
        public ReadOnlyCollection<Region> Blocks { get; }
        public ReadOnlyCollection<ReadOnlyCollection<Region>> Regions { get; }

        public BindingList<string> Actions { get; } = new BindingList<string>();
        public bool IsCustom { get; }

        private readonly Cell[][] _board;

        public Puzzle(int[][] board, bool isCustom,HashSet<int>[][] candidSet=null)
        {
            IsCustom = isCustom;
            _board = Utils.CreateJaggedArray<Cell[][]>(9, 9);

            InitializeAllCells(board,candidSet);

            Region[] rows = new Region[9],
                columns = new Region[9],
                blocks = new Region[9];
            for (int i = 0; i < 9; i++)
            {
                var cells = new Cell[9];

                // initialize row region
                for (int row = 0; row < 9; row++)
                {
                    cells[row] = _board[row][i];
                }
                rows[i] = new Region(cells);

                cells = new Cell[9];
                //initialize column region
                for (int cell = 0; cell < 9; cell++)
                {
                    cells[cell] = _board[i][cell];
                }
                columns[i] = new Region(cells);

                cells = new Cell[9];
                // initialize block region
                int c = 0;
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
            Regions = new ReadOnlyCollection<ReadOnlyCollection<Region>>(new[]
            {
                Rows = new ReadOnlyCollection<Region>(rows),
                Columns = new ReadOnlyCollection<Region>(columns),
                Blocks = new ReadOnlyCollection<Region>(blocks)
            });
        }

        private void InitializeAllCells(int[][] board,HashSet<int>[][] candidSet)
        {
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    _board[x][y] = new Cell(this, board[x][y], new SPoint(x, y),candidSet?[x][y]);
                }
            }
        }

        public Puzzle Clone()
        {
            int[][] board = new int[9][];
            HashSet<int>[][] candidSet = new HashSet<int>[9][];
            for (int i = 0; i < 9; i++)
            {
                board[i] = new int[9];
                candidSet[i] = new HashSet<int>[9];
                for (int j = 0; j < 9; j++)
                {
                    board[i][j] = _board[i][j].Value;
                    candidSet[i][j] = _board[i][j].CloneCandidates();
                }
            }

            return new Puzzle(board, true, candidSet);
        }

        public Cell this[int x, int y] => _board[x][y];

        // Add/Remove the following candidates at the following locations
        /// <summary>
        /// use addCandidate or removeCandidates methods for better understanding
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="candidates"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        [Obsolete]
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

        public bool AddCandidates(IEnumerable<Cell> cells, IEnumerable<int> candidates)
        {
            bool changed = false;
            foreach (Cell cell in cells)
            {
                foreach (int value in candidates)
                {
                    if (cell.Candidates.Add(value))
                    {
                        changed = true;
                    }
                }
            }
            return changed;
        }

        public bool RemoveCandidates(IEnumerable<Cell> cells, IEnumerable<int> candidates)
        {
            bool changed = false;
            foreach (Cell cell in cells)
            {
                foreach (int value in candidates)
                {
                    if (cell.RemoveCandidate(value))
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
