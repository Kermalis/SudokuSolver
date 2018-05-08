using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SudokuSolver.Core
{
    public enum SudokuRegion
    {
        Row,
        Column,
        Block
    }

    public class Region
    {
        public readonly Point[] Points;
        public readonly Cell[] Cells;

        public Region(Board board, SudokuRegion region, int index)
        {
            switch (region)
            {
                case SudokuRegion.Block:
                    Points = new Point[9];
                    // block 0 = 0,0, 0,1, 0,2, 1,0, 1,1, 1,2, 2,0, 2,1, 2,2
                    // block 2 = 6,0, 6,1, 6,2, 7,0, 7,1, 7,2, 8,0, 8,1, 8,2
                    // block 4 = 3,3, 3,4, 3,5, 4,3, 4,4, 4,5, 5,3, 5,4, 5,5
                    // block 8 = 6,6, 6,7, 6,8, 7,6, 7,7, 7,8, 8,6, 8,7, 8,8
                    int ix = (index % 3) * 3, iy = (index / 3) * 3;
                    int c = 0;
                    for (int i = ix; i < ix + 3; i++)
                    {
                        for (int j = iy; j < iy + 3; j++)
                        {
                            Points[c++] = new Point(i, j);
                        }
                    }
                    break;
                case SudokuRegion.Row:
                    Points = new Point[9];
                    for (int i = 0; i < 9; i++)
                        Points[i] = new Point(i, index);
                    break;
                case SudokuRegion.Column:
                    Points = new Point[9];
                    for (int i = 0; i < 9; i++)
                        Points[i] = new Point(index, i);
                    break;
            }
            Cells = Points.Select(p => board[p]).ToArray();
        }

        public int[] GetRegion() => Cells.Select(c => c.Value).ToArray();
        public HashSet<int>[] GetCandidates() => Cells.Select(c => c.Candidates).ToArray();

        public Cell[] GetCellsWithCandidate(int value) => Cells.Where(c => c.Candidates.Contains(value)).ToArray();
        public Point[] GetPointsWithCandidate(int value) => GetCellsWithCandidate(value).Select(c => c.Coordinate).ToArray();
    }
}
