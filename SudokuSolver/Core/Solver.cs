using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace SudokuSolver.Core
{
    class Solver
    {
        int[][] board;
        int[][][] candidates;
        Dictionary<Point, HashSet<int>> blacklist; // A table of blacklisted values from more complicated logic

        Dictionary<SudokuRegion, Region[]> regions;

        string log = "";

        string[] tupleStr = new string[] { "", "single", "pair", "triple", "quadruple" };

        public Solver(int[][] inBoard, UI.SudokuBoard control)
        {
            board = inBoard;
            candidates = Utils.CreateJaggedArray<int[][][]>(9, 9, 9);
            blacklist = new Dictionary<Point, HashSet<int>>();
            control.SetBoard(board, candidates);
            regions = new Dictionary<SudokuRegion, Region[]>(3);
            foreach (SudokuRegion region in Enum.GetValues(typeof(SudokuRegion)))
            {
                var rs = new Region[9];
                for (int i = 0; i < 9; i++)
                {
                    rs[i] = new Region(region, i, board, candidates);
                }
                regions.Add(region, rs);
            }
        }

        private void SetValue(Point p, int value) => SetValue(p.X, p.Y, value);
        private void SetValue(int x, int y, int value)
        {
            board[x][y] = value;
            candidates[x][y] = new int[9]; // Basically setting all candidates to 0 here
        }

        private void Log(string technique, string format, params object[] args) => Log(technique + "\t" + format, args);
        private void Log(string format, params object[] args) => Log(string.Format(format, args));
        private void Log(string s) => log += s + Environment.NewLine;

        private int GetBlock(Point p) => (p.X / 3) + (3 * (p.Y / 3));

        public void DoWork(object sender, DoWorkEventArgs e)
        {
            bool changed, done;
            do
            {
                changed = false; // If this is true at the end of the loop, loop again
                done = true; // If this is true after a segment, the puzzle is solved and we can break

                Log("Loop");

                // Update candidates, then check for naked singles
                for (int i = 0; i < 9; i++)
                {
                    int[] column = regions[SudokuRegion.Column][i].GetRegion();
                    for (int j = 0; j < 9; j++)
                    {
                        if (board[i][j] != 0) continue;

                        Point point = new Point(i, j);
                        int[] row = regions[SudokuRegion.Row][j].GetRegion(), block = regions[SudokuRegion.Block][GetBlock(point)].GetRegion();
                        for (int v = 1; v <= 9; v++)
                        {
                            if (!blacklist.TryGetValue(point, out HashSet<int> specials)) specials = new HashSet<int>();
                            if (!column.Contains(v) && !row.Contains(v) && !block.Contains(v) && !specials.Contains(v))
                            {
                                candidates[i][j][v - 1] = v;
                                done = false;
                            }
                            else
                            {
                                candidates[i][j][v - 1] = 0;
                            }
                        }
                        // Check for naked singles
                        var p = candidates[i][j].Where(v => v != 0).ToArray();
                        if (p.Length == 1)
                        {
                            SetValue(point, p[0]);
                            Log("Naked single", "{0}: {1}", point, p[0]);
                            changed = true;
                            continue;
                        }
                    }
                }
                if (done)
                {
                    Log("Solver completed the puzzle.");
                    break;
                }
                if (changed) continue;

                // Check for hidden singles
                // Faster than "FindHidden" obviously
                for (int i = 0; i < 9; i++)
                {
                    Region r = regions[SudokuRegion.Block][i];
                    for (int v = 1; v <= 9; v++)
                    {
                        Point[] p = r.GetPointsWithCandidate(v);
                        if (p.Length == 1)
                        {
                            SetValue(p[0], v);
                            Log("Hidden single", "{0}: {1}", p[0], v);
                            changed = true;
                        }
                    }
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // Check for X-Wings
                for (int v = 1; v <= 9; v++)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        Region row1 = regions[SudokuRegion.Row][i], col1 = regions[SudokuRegion.Column][i];
                        Point[] row1P = row1.GetPointsWithCandidate(v), col1P = col1.GetPointsWithCandidate(v);
                        for (int j = i + 1; j < 9; j++)
                        {
                            Region row2 = regions[SudokuRegion.Row][j], col2 = regions[SudokuRegion.Column][j];
                            Point[] row2P = row2.GetPointsWithCandidate(v), col2P = col2.GetPointsWithCandidate(v);

                            if (row1P.Length == 2 && row2P.Length == 2 && row1P[0].X == row2P[0].X && row1P[1].X == row2P[1].X)
                            {
                                if (BlacklistCandidates(regions[SudokuRegion.Column][row1P[0].X].Points.Union(regions[SudokuRegion.Column][row1P[1].X].Points).Except(row1P).Except(row2P), new int[] { v }))
                                {
                                    changed = true;
                                    Log("X-Wing", "{0} & {1}: {2}", row1P.Print(), row2P.Print(), v);
                                }
                            }
                            if (col1P.Length == 2 && col2P.Length == 2 && col1P[0].Y == col2P[0].Y && col1P[1].Y == col2P[1].Y)
                            {
                                if (BlacklistCandidates(regions[SudokuRegion.Row][col1P[0].Y].Points.Union(regions[SudokuRegion.Row][col1P[1].Y].Points).Except(col1P).Except(col2P), new int[] { v }))
                                {
                                    changed = true;
                                    Log("X-Wing", "{0} & {1}: {2}", col1P.Print(), col2P.Print(), v);
                                }
                            }
                        }
                    }
                }
                if (changed) continue;

                #region Locked candidates

                // Check for locked row/column candidates
                for (int i = 0; i < 9; i++)
                {
                    Region row = regions[SudokuRegion.Row][i], col = regions[SudokuRegion.Column][i];
                    int[][] rowCand = row.GetCandidates(), colCand = col.GetCandidates();
                    for (int v = 1; v <= 9; v++)
                    {
                        var rowWith = row.GetPointsWithCandidate(v);
                        var colWith = col.GetPointsWithCandidate(v);

                        // Even if a block only has these candidates for this "k" value, it'd be slower to check that before cancelling "BlacklistCandidates"
                        if (rowWith.Count() == 3 || rowWith.Count() == 2)
                        {
                            var blocks = rowWith.Select(p => GetBlock(p)).Distinct().ToArray();
                            if (blocks.Length == 1)
                                if (BlacklistCandidates(regions[SudokuRegion.Block][blocks[0]].Points.Except(rowWith), new int[] { v }))
                                {
                                    changed = true;
                                    Log("Locked candidate", "Row {0} locks block {1}, {2}: {3}", i, blocks[0], rowWith.Print(), v);
                                }
                        }
                        if (colWith.Count() == 3 || colWith.Count() == 2)
                        {
                            var blocks = colWith.Select(p => GetBlock(p)).Distinct().ToArray();
                            if (blocks.Length == 1)
                                if (BlacklistCandidates(regions[SudokuRegion.Block][blocks[0]].Points.Except(colWith), new int[] { v }))
                                {
                                    changed = true;
                                    Log("Locked candidate", "Column {0} locks block {1}, {2}: {3}", i, blocks[0], colWith.Print(), v);
                                }
                        }
                    }
                }
                if (changed) continue;

                // Check for pointing pairs/triples
                // For example: 
                // 9 3 6     0 5 0     7 0 4
                // 2 7 8     1 9 4     5 3 6
                // 0 0 5     0 7 0     9 0 0
                // The block on the left can only have 1s in the bottom row, so remove the possibility of 1s in the block on the right's bottom row
                // A 1 will then be placed in the top spot of that block on the next loop, because it is the only available spot for a 1
                for (int i = 0; i < 3; i++)
                {
                    Point[][] blockrow = new Point[3][], blockcol = new Point[3][];
                    for (int r = 0; r < 3; r++)
                    {
                        blockrow[r] = regions[SudokuRegion.Block][r + (i * 3)].Points;
                        blockcol[r] = regions[SudokuRegion.Block][i + (r * 3)].Points;
                    }
                    for (int r = 0; r < 3; r++) // 3 blocks in a blockrow/blockcolumn
                    {
                        int[][] rowCand = new int[3][], colCand = new int[3][];
                        for (int j = 0; j < 3; j++) // 3 rows/columns in block
                        {
                            // The 3 cells' candidates in a block's row/column
                            List<int> thingyr = new List<int>(27), thingyc = new List<int>(27);
                            foreach (int[] cell in blockrow[r].GetRow(j).Select(p => candidates[p.X][p.Y]))
                                thingyr.AddRange(cell);
                            foreach (int[] cell in blockcol[r].GetColumn(j).Select(p => candidates[p.X][p.Y]))
                                thingyc.AddRange(cell);
                            rowCand[j] = thingyr.Distinct().Where(v => v != 0).ToArray();
                            colCand[j] = thingyc.Distinct().Where(v => v != 0).ToArray();
                        }
                        // Now check if a row has a distinct candidate
                        var zero_distinct = rowCand[0].Except(rowCand[1]).Except(rowCand[2]);
                        if (zero_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockrow, true, i, r, 0, zero_distinct)) changed = true;
                        var one_distinct = rowCand[1].Except(rowCand[0]).Except(rowCand[2]);
                        if (one_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockrow, true, i, r, 1, one_distinct)) changed = true;
                        var two_distinct = rowCand[2].Except(rowCand[0]).Except(rowCand[1]);
                        if (two_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockrow, true, i, r, 2, two_distinct)) changed = true;
                        // Now check if a column has a distinct candidate
                        zero_distinct = colCand[0].Except(colCand[1]).Except(colCand[2]);
                        if (zero_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockcol, false, i, r, 0, zero_distinct)) changed = true;
                        one_distinct = colCand[1].Except(colCand[0]).Except(colCand[2]);
                        if (one_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockcol, false, i, r, 1, one_distinct)) changed = true;
                        two_distinct = colCand[2].Except(colCand[0]).Except(colCand[1]);
                        if (two_distinct.Count() > 0)
                            if (RemoveBlockRowColCandidates(blockcol, false, i, r, 2, two_distinct)) changed = true;
                    }
                }
                if (changed) continue;

                #endregion

                #region Naked tuples & Hidden tuples

                // Check for naked pairs
                for (int i = 0; i < 9; i++)
                {
                    if (FindNaked(regions[SudokuRegion.Block][i], 2)
                        || FindNaked(regions[SudokuRegion.Row][i], 2)
                        || FindNaked(regions[SudokuRegion.Column][i], 2)) changed = true;
                }
                if (changed) continue;
                // Check for hidden pairs
                for (int i = 0; i < 9; i++)
                {
                    if (FindHidden(regions[SudokuRegion.Block][i], 2)
                        || FindHidden(regions[SudokuRegion.Row][i], 2)
                        || FindHidden(regions[SudokuRegion.Column][i], 2)) changed = true;
                }
                if (changed) continue;

                // Check for naked triples
                for (int i = 0; i < 9; i++)
                {
                    if (FindNaked(regions[SudokuRegion.Block][i], 3)
                        || FindNaked(regions[SudokuRegion.Row][i], 3)
                        || FindNaked(regions[SudokuRegion.Column][i], 3)) changed = true;
                }
                if (changed) continue;
                // Check for hidden triples
                for (int i = 0; i < 9; i++)
                {
                    if (FindHidden(regions[SudokuRegion.Block][i], 3)
                        || FindHidden(regions[SudokuRegion.Row][i], 3)
                        || FindHidden(regions[SudokuRegion.Column][i], 3)) changed = true;
                }
                if (changed) continue;

                // Check for naked quads
                for (int i = 0; i < 9; i++)
                {
                    if (FindNaked(regions[SudokuRegion.Block][i], 4)
                        || FindNaked(regions[SudokuRegion.Row][i], 4)
                        || FindNaked(regions[SudokuRegion.Column][i], 4)) changed = true;
                }
                if (changed) continue;
                // Check for hidden quads
                for (int i = 0; i < 9; i++)
                {
                    if (FindHidden(regions[SudokuRegion.Block][i], 4)
                        || FindHidden(regions[SudokuRegion.Row][i], 4)
                        || FindHidden(regions[SudokuRegion.Column][i], 4)) changed = true;
                }

                #endregion

            } while (changed);

            e.Result = log;
        }

        // Find hidden pairs/triples/quadruples
        private bool FindHidden(Region region, int amt)
        {
            if (region.Points.Count(p => candidates[p.X][p.Y].Distinct().Count() > 1) == amt) // If there are only "amt" cells with non-zero candidates, we don't have to waste our time
                return false;
            return DoHidden(region, new int[amt], 0, amt);
        }
        private bool DoHidden(Region region, int[] cand, int loop, int amt)
        {
            bool changed = false;
            if (loop == amt)
            {
                var cells = new Point[0];
                foreach (int v in cand)
                {
                    cells = cells.Union(region.GetPointsWithCandidate(v)).ToArray();
                }
                if (cells.Length != amt // There aren't "amt" cells for our tuple to be in
                    || cells.Select(p => candidates[p.X][p.Y].Where(v => v != 0)).UniteAll().Count() == amt // We already know it's a tuple (might be faster to skip this check, idk)
                    || cand.Any(v => !cells.Any(p => candidates[p.X][p.Y].Contains(v)))) return false; // If a number in our combo doesn't actually show up in any of our cells
                if (BlacklistCandidates(cells, Enumerable.Range(1, 9).Except(cand)))
                {
                    Log("Hidden " + tupleStr[amt], "{0}: {1}", cells.Print(), cand.Print());
                    return true;
                }
            }
            else
            {
                for (int i = cand[loop == 0 ? loop : loop - 1] + 1; i <= 9; i++)
                {
                    cand[loop] = i;
                    if (DoHidden(region, cand, loop + 1, amt)) changed = true;
                }
            }
            return changed;
        }

        // Find naked pairs/triples/quadruples
        private bool FindNaked(Region region, int amt)
        {
            if (region.Points.Count(p => candidates[p.X][p.Y].Distinct().Count() > 1) == amt) // If there are only "amt" cells with non-zero candidates, we don't have to waste our time
                return false;
            return DoNaked(region, new Point[amt], new int[amt], 0, amt);
        }
        private bool DoNaked(Region region, Point[] points, int[] cand, int loop, int amt)
        {
            bool changed = false;
            if (loop == amt)
            {
                var combo = points.Select(p => candidates[p.X][p.Y]).UniteAll().Where(v => v != 0).ToArray();
                if (combo.Length == amt)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (cand.Contains(i)) continue; // Don't blacklist in our tuple's cells
                        if (BlacklistCandidates(new Point[] { region.Points[i] }, combo)) changed = true;
                    }
                    if (changed) Log("Naked " + tupleStr[amt], "{0}: {1}", points.Print(), combo.Print());
                }
            }
            else
            {
                for (int i = loop == 0 ? 0 : cand[loop - 1] + 1; i < 9; i++)
                {
                    Point p = region.Points[i];
                    if (candidates[p.X][p.Y].Distinct().Count() == 1) continue; // Only 0s
                    points[loop] = p;
                    cand[loop] = i;
                    if (DoNaked(region, points, cand, loop + 1, amt)) changed = true;
                }
            }
            return changed;
        }

        // Clear candidates from a blockrow/blockcolumn and return true if something changed
        private bool RemoveBlockRowColCandidates(Point[][] blockrcs, bool doRows, int current, int ignoreBlock, int rc, IEnumerable<int> cand)
        {
            // Not optimized
            bool changed = false;
            for (int i = 0; i < 3; i++)
            {
                if (i == ignoreBlock) continue;
                var rcs = doRows ? blockrcs[i].GetRow(rc) : blockrcs[i].GetColumn(rc);
                if (BlacklistCandidates(rcs, cand)) changed = true;
            }
            if (changed) Log("Pointing couple", "Starting in block{0} {1}'s block {2}, {0} {3}: {4}", doRows ? "row" : "column", current, ignoreBlock, rc, cand.Print());
            return changed;
        }

        // Blacklist the following candidates at the following cells
        private bool BlacklistCandidates(IEnumerable<Point> points, IEnumerable<int> cand)
        {
            bool changed = false;
            foreach (Point p in points)
            {
                foreach (int v in cand)
                {
                    if (candidates[p.X][p.Y][v - 1] != 0)
                    {
                        changed = true;
                        candidates[p.X][p.Y][v - 1] = 0;
                        if (!blacklist.ContainsKey(p)) blacklist.Add(p, new HashSet<int>());
                        blacklist[p].Add(v);
                    }
                }
            }
            return changed;
        }
    }
}
