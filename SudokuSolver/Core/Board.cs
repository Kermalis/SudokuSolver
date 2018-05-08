using System.Collections.Generic;

namespace SudokuSolver.Core
{
    public class Board
    {
        public static Region[] Rows { get; private set; }
        public static Region[] Columns { get; private set; }
        public static Region[] Blocks { get; private set; }
        public static Region[][] Regions { get; private set; }

        Cell[][] _board;

        public Board(int[][] inBoard)
        {
            _board = Utils.CreateJaggedArray<Cell[][]>(9, 9);
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
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    if (_board[x][y] != 0) _board[x][y].Set(_board[x][y]); // Update candidates
        }

        public Cell this[int x, int y]
        {
            get => _board[x][y];
        }
        public Cell this[SPoint p]
        {
            get => _board[p.X][p.Y];
        }

        // Blacklist the following candidates at the following cells
        public bool BlacklistCandidates(IEnumerable<SPoint> points, IEnumerable<int> cand)
        {
            bool changed = false;
            foreach (SPoint p in points)
                foreach (int v in cand)
                    if (this[p].Candidates.Remove(v)) changed = true;
            return changed;
        }
    }
}
