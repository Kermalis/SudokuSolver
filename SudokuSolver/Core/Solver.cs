using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SudokuSolver.Core
{
    class Solver
    {
        byte[][] board;
        byte[][][] candidates;
        Dictionary<Point, List<byte>> specials; // A table of blacklisted values from more complicated logic
        SudokuSolver.UI.SudokuBoard sudokuBoard;

        public Solver(SudokuSolver.UI.SudokuBoard control, byte[][] inBoard)
        {
            sudokuBoard = control;
            board = inBoard;
            candidates = Utils.CreateJaggedArray<byte[][][]>(9, 9, 9);
            specials = new Dictionary<Point, List<byte>>();
            sudokuBoard.SetBoard(board, candidates);
        }

        private void SetValue(Point p, byte value) => SetValue((byte)p.X, (byte)p.Y, value);
        private void SetValue(byte x, byte y, byte value)
        {
            board[x][y] = value;
            //candidates[x][y] = new byte[9]; // Basically setting all candidates to 0 here
        }

        private byte CoordsToBlock(byte x, byte y) => (byte)((x / 3) + (3 * (y / 3)));

        private Point[] GetPointsInBlock(byte x, byte y) => GetPointsInBlock(CoordsToBlock(x, y));
        private Point[] GetPointsInBlock(byte block)
        {
            var points = new Point[9];
            // block 0 = 0,0, 0,1, 0,2, 1,0, 1,1, 1,2, 2,0, 2,1, 2,2
            // block 2 = 6,0, 6,1, 6,2, 7,0, 7,1, 7,2, 8,0, 8,1, 8,2
            // block 4 = 3,3, 3,4, 3,5, 4,3, 4,4, 4,5, 5,3, 5,4, 5,5
            // block 8 = 6,6, 6,7, 6,8, 7,6, 7,7, 7,8, 8,6, 8,7, 8,8
            int ix = (block % 3) * 3, iy = (block / 3) * 3;
            int c = 0;
            for (int i = ix; i < ix + 3; i++)
            {
                for (int j = iy; j < iy + 3; j++)
                {
                    points[c++] = new Point(i, j);
                }
            }
            return points;
        }
        private Point[] GetPointsInRow(byte row)
        {
            var points = new Point[9];
            for (byte i = 0; i < 9; i++)
                points[i] = new Point(i, row);
            return points;
        }
        private Point[] GetPointsInColumn(byte column)
        {
            var points = new Point[9];
            for (byte i = 0; i < 9; i++)
                points[i] = new Point(column, i);
            return points;
        }

        private byte[] GetBlock(byte x, byte y) => GetBlock(CoordsToBlock(x, y));
        private byte[] GetBlock(byte block)
        {
            var bl = new byte[9];
            var points = GetPointsInBlock(block);
            for (int i = 0; i < 9; i++)
                bl[i] = board[points[i].X][points[i].Y];
            return bl;
        }

        public void Begin()
        {
            bool changed, done;
            do
            {
                changed = false; // If this is true at the end of the loop, loop again
                done = true; // If this is true after a segment, the puzzle is solved and we can break

                // Update candidates, then check for naked singles
                for (byte i = 0; i < 9; i++)
                {
                    byte[] column = board.GetColumn(i);
                    for (byte j = 0; j < 9; j++)
                    {
                        if (board[i][j] != 0) { candidates[i][j] = new byte[9]; continue; }; // TODO: Optimize

                        byte[] row = board.GetRow(j), block = GetBlock(i, j);
                        Point point = new Point(i, j);
                        for (byte k = 1; k <= 9; k++)
                        {
                            if (!specials.TryGetValue(point, out List<byte> blacklist)) blacklist = new List<byte>();
                            if (!column.Contains(k) && !row.Contains(k) && !block.Contains(k) && !blacklist.Contains(k))
                            {
                                candidates[i][j][k - 1] = k;
                                done = false;
                            }
                            else
                            {
                                candidates[i][j][k - 1] = 0;
                            }
                        }
                        // Check for naked singles
                        var p = candidates[i][j].Where(b => b != 0).ToArray();
                        if (p.Length == 1)
                        {
                            SetValue(point, p[0]);
                            changed = true;
                            continue;
                        }
                    }
                }
                if (done) break;

                // Check for hidden singles
                for (byte i = 0; i < 9; i++)
                {
                    Point[] points = GetPointsInBlock(i);
                    for (byte k = 1; k <= 9; k++)
                    {
                        Point[] p = points.Where(po => candidates[po.X][po.Y].Contains(k)).ToArray();
                        if (p.Length == 1)
                        {
                            SetValue(p[0], k);
                            changed = true;
                        }
                    }
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // Check for blockrows/blockcolumns that have a row/column that must have a number, to remove that number from the other blocks in that blockrow/blockcolumn
                // For example: 
                // 9 3 6     0 5 0     7 0 4
                // 2 7 8     1 9 4     5 3 6
                // 0 0 5     0 7 0     9 0 0
                // The block on the left can only have 1s in the bottom row, so remove the possibility of 1s in the block on the right's bottom row
                // A 1 will then be placed in the top spot of that block on the next loop, because it is the only available spot for a 1
                for (byte i = 0; i < 3; i++)
                {
                    Point[][] blockrow = new Point[3][], blockcol = new Point[3][];
                    for (byte r = 0; r < 3; r++)
                    {
                        blockrow[r] = GetPointsInBlock((byte)(r + (i * 3)));
                        blockcol[r] = GetPointsInBlock((byte)(i + (r * 3)));
                    }
                    for (byte r = 0; r < 3; r++) // 3 blocks in a blockrow/blockcolumn
                    {
                        byte[][] rowCand = new byte[3][], colCand = new byte[3][];
                        for (byte j = 0; j < 3; j++) // 3 rows/columns in block
                        {
                            // The 3 cells' candidates in a block's row/column
                            List<byte> thingyr = new List<byte>(27), thingyc = new List<byte>(27);
                            foreach (byte[] cell in blockrow[r].GetRow(j).Select(p => candidates[p.X][p.Y]))
                                thingyr.AddRange(cell);
                            foreach (byte[] cell in blockcol[r].GetColumn(j).Select(p => candidates[p.X][p.Y]))
                                thingyc.AddRange(cell);
                            rowCand[j] = thingyr.Distinct().Where(b => b != 0).ToArray();
                            colCand[j] = thingyc.Distinct().Where(b => b != 0).ToArray();
                        }
                        // Now check if a row has a distinct candidate
                        var zero_distinct = rowCand[0].Except(rowCand[1]).Except(rowCand[2]);
                        if (zero_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockrow, true, r, 0, zero_distinct)) changed = true;
                        var one_distinct = rowCand[1].Except(rowCand[0]).Except(rowCand[2]);
                        if (one_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockrow, true, r, 1, one_distinct)) changed = true;
                        var two_distinct = rowCand[2].Except(rowCand[0]).Except(rowCand[1]);
                        if (two_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockrow, true, r, 2, two_distinct)) changed = true;
                        // Now check if a column has a distinct candidate
                        zero_distinct = colCand[0].Except(colCand[1]).Except(colCand[2]);
                        if (zero_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockcol, false, r, 0, zero_distinct)) changed = true;
                        one_distinct = colCand[1].Except(colCand[0]).Except(colCand[2]);
                        if (one_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockcol, false, r, 1, one_distinct)) changed = true;
                        two_distinct = colCand[2].Except(colCand[0]).Except(colCand[1]);
                        if (two_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockcol, false, r, 2, two_distinct)) changed = true;
                    }
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // TODO: Instead of a bunch of unoptimized "GetPointsIn," have these point arrays in an array
                // Check for naked pairs
                for (byte i = 0; i < 9; i++)
                {
                    if (DoNakedPairs(GetPointsInBlock(i))) changed = true;
                    if (DoNakedPairs(GetPointsInRow(i))) changed = true;
                    if (DoNakedPairs(GetPointsInColumn(i))) changed = true;
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // Check for hidden pairs
                for (byte i = 0; i < 9; i++)
                {
                    if (DoHiddenPairs(GetPointsInBlock(i))) changed = true;
                    if (DoHiddenPairs(GetPointsInRow(i))) changed = true;
                    if (DoHiddenPairs(GetPointsInColumn(i))) changed = true;
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // Check for naked triples
                for (byte i = 0; i < 9; i++)
                {
                    if (DoNakedTriples(GetPointsInBlock(i))) changed = true;
                    if (DoNakedTriples(GetPointsInRow(i))) changed = true;
                    if (DoNakedTriples(GetPointsInColumn(i))) changed = true;
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // Check for naked quads
                for (byte i = 0; i < 9; i++)
                {
                    if (DoNakedQuads(GetPointsInBlock(i))) changed = true;
                    if (DoNakedQuads(GetPointsInRow(i))) changed = true;
                    if (DoNakedQuads(GetPointsInColumn(i))) changed = true;
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // Check for hidden quads
                /*for (byte i = 0; i < 9; i++)
                {
                    if (DoHiddenQuads(GetPointsInBlock(i))) changed = true;
                    if (DoHiddenQuads(GetPointsInRow(i))) changed = true;
                    if (DoHiddenQuads(GetPointsInColumn(i))) changed = true;
                }*/
            } while (changed);
            sudokuBoard.Invalidate();
        }

        // This is a copy-pasted "DoHiddenPairs" which will only work if all 4 values are in all 4 cells
        private bool DoHiddenQuads(Point[] points)
        {
            bool changed = false;
            var hidden = new Dictionary<byte, Point[]>(4);
            for (byte k = 1; k <= 9; k++)
            {
                if (hidden.Count < 4)
                {
                    Point[] p = points.Where(po => candidates[po.X][po.Y].Contains(k)).ToArray();
                    if (p.Length == 4)
                    {
                        hidden.Add(k, p);
                    }
                }
                else break;
            }
            if (hidden.Count == 4 && Utils.AreAllSequencesEqual(hidden.Values.ToArray()))
            {
                if (BlacklistCandidates(points.Except(hidden.Values.ElementAt(0)), hidden.Keys)) changed = true;
            }
            return changed;
        }

        // TODO: Make these a better function
        private bool DoHiddenPairs(Point[] points)
        {
            bool changed = false;
            var hidden = new Dictionary<byte, Point[]>(2);
            for (byte k = 1; k <= 9; k++)
            {
                if (hidden.Count < 2)
                {
                    Point[] p = points.Where(po => candidates[po.X][po.Y].Contains(k)).ToArray();
                    if (p.Length == 2)
                    {
                        hidden.Add(k, p);
                    }
                }
                else break;
            }
            if (hidden.Count == 2 && Utils.AreAllSequencesEqual(hidden.Values.ToArray()))
            {
                if (BlacklistCandidates(points.Except(hidden.Values.ElementAt(0)), hidden.Keys)) changed = true;
            }
            return changed;
        }

        // TODO: Change into recursion
        private bool DoNakedQuads(Point[] points)
        {
            bool changed = false;
            for (byte j = 0; j < 9; j++)
            {
                Point pj = points[j];
                if (candidates[pj.X][pj.Y].Distinct().Count() == 1) continue;
                for (int k = j + 1; k < 9; k++)
                {
                    Point pk = points[k];
                    if (candidates[pk.X][pk.Y].Distinct().Count() == 1) continue;
                    for (int l = k + 1; l < 9; l++)
                    {
                        Point pl = points[l];
                        if (candidates[pl.X][pl.Y].Distinct().Count() == 1) continue;
                        for (int m = l + 1; m < 9; m++)
                        {
                            Point pm = points[m];
                            if (candidates[pm.X][pm.Y].Distinct().Count() == 1) continue;
                            var cand = candidates[pj.X][pj.Y].Union(candidates[pk.X][pk.Y]).Union(candidates[pl.X][pl.Y]).Union(candidates[pm.X][pm.Y]).Where(b => b != 0);
                            if (cand.Count() == 4)
                            {
                                for (byte i = 0; i < 9; i++)
                                {
                                    if (j == i || k == i || l == i || m == i) continue; // Don't blacklist in our quad's cells
                                    if (BlacklistCandidates(points[i], cand)) changed = true;
                                }
                            }
                        }
                    }
                }
            }
            return changed;
        }

        // TODO: Change into recursion
        private bool DoNakedTriples(Point[] points)
        {
            bool changed = false;
            for (byte j = 0; j < 9; j++)
            {
                Point pj = points[j];
                if (candidates[pj.X][pj.Y].Distinct().Count() == 1) continue;
                for (int k = j + 1; k < 9; k++)
                {
                    Point pk = points[k];
                    if (candidates[pk.X][pk.Y].Distinct().Count() == 1) continue;
                    for (int l = k + 1; l < 9; l++)
                    {
                        Point pl = points[l];
                        if (candidates[pl.X][pl.Y].Distinct().Count() == 1) continue;
                        var cand = candidates[pj.X][pj.Y].Union(candidates[pk.X][pk.Y]).Union(candidates[pl.X][pl.Y]).Where(b => b != 0);
                        if (cand.Count() == 3)
                        {
                            for (byte i = 0; i < 9; i++)
                            {
                                if (j == i || k == i || l == i) continue; // Don't blacklist in our triple's cells
                                if (BlacklistCandidates(points[i], cand)) changed = true;
                            }
                        }
                    }
                }
            }
            return changed;
        }

        private bool DoNakedPairs(Point[] points)
        {
            var cand = points.Select(p => candidates[p.X][p.Y]).ToArray();
            for (byte j = 0; j < cand.Length; j++)
                cand[j] = cand[j].Distinct().Where(b => b != 0).ToArray();
            bool changed = false;
            for (byte j = 0; j < cand.Length; j++)
            {
                if (cand[j].Length != 2) continue;
                for (int k = j + 1; k < cand.Length; k++)
                {
                    if (cand[j].SequenceEqual(cand[k])) // Two cells in a block have the same candidates
                    {
                        for (byte i = 0; i < 9; i++)
                        {
                            if (j == i || k == i) continue; // Don't blacklist in our pair's cells
                            if (BlacklistCandidates(points[i], cand[j])) changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        // Clear candidates from a blockrow/blockcolumn and return true if something changed
        private bool RemoveBlockRowColCandidates(Point[][] blockrcs, bool doRows, byte ignoreBlock, byte rc, IEnumerable<byte> cand)
        {
            // Not optimized
            bool changed = false;
            for (byte i = 0; i < 3; i++)
            {
                if (i == ignoreBlock) continue;
                var rcs = doRows ? blockrcs[i].GetRow(rc) : blockrcs[i].GetColumn(rc);
                if (BlacklistCandidates(rcs, cand)) changed = true;
            }
            return changed;
        }

        // Blacklist the following candidates at the following cells
        private bool BlacklistCandidates(Point p, IEnumerable<byte> cand) => BlacklistCandidates(new Point[] { p }, cand);
        private bool BlacklistCandidates(IEnumerable<Point> points, IEnumerable<byte> cand)
        {
            bool changed = false;
            foreach (Point p in points)
            {
                foreach (byte b in cand)
                {
                    if (candidates[p.X][p.Y][b - 1] != 0)
                    {
                        changed = true;
                        candidates[p.X][p.Y][b - 1] = 0;
                        if (!specials.ContainsKey(p)) specials.Add(p, new List<byte>());
                        specials[p].Add(b);
                    }
                }
            }
            return changed;
        }
    }
}
