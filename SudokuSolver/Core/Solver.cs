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
        Dictionary<Point, List<int>> specials; // A table of blacklisted values from more complicated logic

        Dictionary<SudokuRegion, Region[]> regions;

        string log = "";

        public Solver(int[][] inBoard, UI.SudokuBoard control)
        {
            board = inBoard;
            candidates = Utils.CreateJaggedArray<int[][][]>(9, 9, 9);
            specials = new Dictionary<Point, List<int>>();
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
                        for (int k = 1; k <= 9; k++)
                        {
                            if (!specials.TryGetValue(point, out List<int> blacklist)) blacklist = new List<int>();
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
                for (int i = 0; i < 9; i++)
                {
                    for (int k = 1; k <= 9; k++)
                    {
                        Point[] p = regions[SudokuRegion.Block][i].Points.Where(po => candidates[po.X][po.Y].Contains(k)).ToArray();
                        if (p.Length == 1)
                        {
                            SetValue(p[0], k);
                            Log("Hidden single", "{0}: {1}", p[0], k);
                            changed = true;
                        }
                    }
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // Check for locked row/column candidates
                for (int i = 0; i < 9; i++)
                {
                    Region row = regions[SudokuRegion.Row][i], col = regions[SudokuRegion.Column][i];
                    int[][] rowCand = row.GetCandidates(), colCand = col.GetCandidates();
                    for (int k = 1; k <= 9; k++)
                    {
                        var rowWith = row.Points.Where(p => candidates[p.X][p.Y].Contains(k));
                        var colWith = col.Points.Where(p => candidates[p.X][p.Y].Contains(k));

                        // Even if a block only has these candidates for this "k" value, it'd be slower to check that before cancelling "BlacklistCandidates"
                        if (rowWith.Count() == 3 || rowWith.Count() == 2)
                        {
                            var blocks = rowWith.Select(p => GetBlock(p)).Distinct().ToArray();
                            if (blocks.Length == 1)
                                if (BlacklistCandidates(regions[SudokuRegion.Block][blocks[0]].Points.Except(rowWith), new int[] { k }))
                                {
                                    changed = true;
                                    Log("Locked candidate", "Row {0} locks block {1}, {2}: {3}", i, blocks[0], rowWith.Print(), k);
                                }
                        }
                        if (colWith.Count() == 3 || colWith.Count() == 2)
                        {
                            var blocks = colWith.Select(p => GetBlock(p)).Distinct().ToArray();
                            if (blocks.Length == 1)
                                if (BlacklistCandidates(regions[SudokuRegion.Block][blocks[0]].Points.Except(colWith), new int[] { k }))
                                {
                                    changed = true;
                                    Log("Locked candidate", "Column {0} locks block {1}, {2}: {3}", i, blocks[0], colWith.Print(), k);
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
                            rowCand[j] = thingyr.Distinct().Where(b => b != 0).ToArray();
                            colCand[j] = thingyc.Distinct().Where(b => b != 0).ToArray();
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

                // Check for naked pairs
                for (int i = 0; i < 9; i++)
                {
                    if (DoNakedPairs(regions[SudokuRegion.Block][i])) changed = true;
                    if (DoNakedPairs(regions[SudokuRegion.Row][i])) changed = true;
                    if (DoNakedPairs(regions[SudokuRegion.Column][i])) changed = true;
                }
                if (changed) continue;
                // Check for hidden pairs
                for (int i = 0; i < 9; i++)
                {
                    if (DoHiddenPairs(regions[SudokuRegion.Block][i])) changed = true;
                    if (DoHiddenPairs(regions[SudokuRegion.Row][i])) changed = true;
                    if (DoHiddenPairs(regions[SudokuRegion.Column][i])) changed = true;
                }
                if (changed) continue;

                // Check for naked triples
                for (int i = 0; i < 9; i++)
                {
                    if (DoNakedTriples(regions[SudokuRegion.Block][i])) changed = true;
                    if (DoNakedTriples(regions[SudokuRegion.Row][i])) changed = true;
                    if (DoNakedTriples(regions[SudokuRegion.Column][i])) changed = true;
                }
                if (changed) continue;
                // Check for hidden triples
                for (int i = 0; i < 9; i++)
                {
                    if (DoHiddenTriples(regions[SudokuRegion.Block][i])) changed = true;
                    if (DoHiddenTriples(regions[SudokuRegion.Row][i])) changed = true;
                    if (DoHiddenTriples(regions[SudokuRegion.Column][i])) changed = true;
                }
                if (changed) continue;

                // Check for naked quads
                for (int i = 0; i < 9; i++)
                {
                    if (DoNakedQuads(regions[SudokuRegion.Block][i])) changed = true;
                    if (DoNakedQuads(regions[SudokuRegion.Row][i])) changed = true;
                    if (DoNakedQuads(regions[SudokuRegion.Column][i])) changed = true;
                }
                if (changed) continue;
                // Check for hidden quads
                for (int i = 0; i < 9; i++)
                {
                    if (DoHiddenQuads(regions[SudokuRegion.Block][i])) changed = true;
                    if (DoHiddenQuads(regions[SudokuRegion.Row][i])) changed = true;
                    if (DoHiddenQuads(regions[SudokuRegion.Column][i])) changed = true;
                }
            } while (changed);

            e.Result = log;
        }

        // TODO: Change into recursion
        private bool DoHiddenQuads(Region region)
        {
            if (region.Points.Count(p => candidates[p.X][p.Y].Distinct().Count() > 1) == 4) // If there are only 4 cells with non-zero candidates, we don't have to waste our time
                return false;
            bool changed = false;
            for (int j = 1; j <= 9; j++)
            {
                for (int k = j + 1; k <= 9; k++)
                {
                    for (int l = k + 1; l <= 9; l++)
                    {
                        for (int m = l + 1; m <= 9; m++)
                        {
                            var combo = new int[] { j, k, l, m };
                            var cells = new Point[0];
                            foreach (int b in combo)
                            {
                                cells = cells.Union(region.Points.Where(p => candidates[p.X][p.Y].Contains(b))).ToArray();
                            }
                            if (cells.Length != 4
                                || cells.Select(p => candidates[p.X][p.Y].Where(b => b != 0)).UniteAll().Count() == 4
                                || combo.Any(b => !cells.Any(p => candidates[p.X][p.Y].Contains(b)))) continue;
                            if (BlacklistCandidates(cells, Enumerable.Range(1, 9).Except(combo))) changed = true;
                            Log("Hidden quadruple", "{0}: {1}", cells.Print(), combo.Print());
                        }
                    }
                }
            }
            return changed;
        }
        private bool DoHiddenTriples(Region region)
        {
            if (region.Points.Count(p => candidates[p.X][p.Y].Distinct().Count() > 1) == 3) // If there are only 3 cells with non-zero candidates, we don't have to waste our time
                return false;
            bool changed = false;
            for (int j = 1; j <= 9; j++)
            {
                for (int k = j + 1; k <= 9; k++)
                {
                    for (int l = k + 1; l <= 9; l++)
                    {
                        var combo = new int[] { j, k, l };
                        var cells = new Point[0];
                        foreach (int b in combo)
                        {
                            cells = cells.Union(region.Points.Where(p => candidates[p.X][p.Y].Contains(b))).ToArray();
                        }
                        if (cells.Length != 3 // There aren't 3 cells for our triple to be in
                            || cells.Select(p => candidates[p.X][p.Y].Where(b => b != 0)).UniteAll().Count() == 3 // We already know it's a triple
                            || combo.Any(b => !cells.Any(p => candidates[p.X][p.Y].Contains(b)))) continue; // If a number in our combo doesn't actually show up in any of our cells
                        if (BlacklistCandidates(cells, Enumerable.Range(1, 9).Except(combo))) changed = true;
                        Log("Hidden triple", "{0}: {1}", cells.Print(), combo.Print());
                    }
                }
            }
            return changed;
        }
        private bool DoHiddenPairs(Region region)
        {
            if (region.Points.Count(p => candidates[p.X][p.Y].Distinct().Count() > 1) == 2) // If there are only 2 cells with non-zero candidates, we don't have to waste our time
                return false;
            bool changed = false;
            var hidden = new Dictionary<int, Point[]>(2);
            for (int k = 1; k <= 9; k++)
            {
                if (hidden.Count < 2)
                {
                    Point[] p = region.Points.Where(po => candidates[po.X][po.Y].Contains(k)).ToArray();
                    if (p.Length == 2)
                    {
                        hidden.Add(k, p);
                    }
                }
                else break;
            }
            var values = hidden.Values.ToArray();
            if (hidden.Count == 2 && Utils.AreAllSequencesEqual(values))
            {
                if (BlacklistCandidates(region.Points.Except(values[0]), hidden.Keys)) changed = true;
                Log("Hidden pair", "{0}: {1}", values[0].Print(), hidden.Keys.Print());
            }
            return changed;
        }

        // TODO: Change into recursion
        private bool DoNakedQuads(Region region)
        {
            if (region.Points.Count(p => candidates[p.X][p.Y].Distinct().Count() > 1) == 4) // If there are only 4 cells with non-zero candidates, we don't have to waste our time
                return false;
            bool changed = false;
            for (int j = 0; j < 9; j++)
            {
                Point pj = region.Points[j];
                if (candidates[pj.X][pj.Y].Distinct().Count() == 1) continue;
                for (int k = j + 1; k < 9; k++)
                {
                    Point pk = region.Points[k];
                    if (candidates[pk.X][pk.Y].Distinct().Count() == 1) continue;
                    for (int l = k + 1; l < 9; l++)
                    {
                        Point pl = region.Points[l];
                        if (candidates[pl.X][pl.Y].Distinct().Count() == 1) continue;
                        for (int m = l + 1; m < 9; m++)
                        {
                            Point pm = region.Points[m];
                            if (candidates[pm.X][pm.Y].Distinct().Count() == 1) continue;
                            var ps = new Point[] { pj, pk, pl, pm };
                            var cand = ps.Select(p => candidates[p.X][p.Y]).UniteAll().Where(b => b != 0).ToArray();
                            if (cand.Length == 4)
                            {
                                for (int i = 0; i < 9; i++)
                                {
                                    if (j == i || k == i || l == i || m == i) continue; // Don't blacklist in our quad's cells
                                    if (BlacklistCandidates(new Point[] { region.Points[i] }, cand)) changed = true;
                                }
                                if (changed) Log("Naked quadruple", "{0}: {1}", ps.Print(), cand.Print());
                            }
                        }
                    }
                }
            }
            return changed;
        }
        private bool DoNakedTriples(Region region)
        {
            if (region.Points.Count(p => candidates[p.X][p.Y].Distinct().Count() > 1) == 3) // If there are only 3 cells with non-zero candidates, we don't have to waste our time
                return false;
            bool changed = false;
            for (int j = 0; j < 9; j++)
            {
                Point pj = region.Points[j];
                if (candidates[pj.X][pj.Y].Distinct().Count() == 1) continue;
                for (int k = j + 1; k < 9; k++)
                {
                    Point pk = region.Points[k];
                    if (candidates[pk.X][pk.Y].Distinct().Count() == 1) continue;
                    for (int l = k + 1; l < 9; l++)
                    {
                        Point pl = region.Points[l];
                        if (candidates[pl.X][pl.Y].Distinct().Count() == 1) continue;
                        var ps = new Point[] { pj, pk, pl };
                        var cand = ps.Select(p => candidates[p.X][p.Y]).UniteAll().Where(b => b != 0).ToArray();
                        if (cand.Length == 3)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                if (j == i || k == i || l == i) continue; // Don't blacklist in our triple's cells
                                if (BlacklistCandidates(new Point[] { region.Points[i] }, cand)) changed = true;
                            }
                            if (changed) Log("Naked triple", "{0}: {1}", ps.Print(), cand.Print());
                        }
                    }
                }
            }
            return changed;
        }
        private bool DoNakedPairs(Region region)
        {
            if (region.Points.Count(p => candidates[p.X][p.Y].Distinct().Count() > 1) == 2) // If there are only 2 cells with non-zero candidates, we don't have to waste our time
                return false;
            var cand = region.GetCandidates();
            for (int j = 0; j < cand.Length; j++)
                cand[j] = cand[j].Distinct().Where(b => b != 0).ToArray();
            bool changed = false;
            for (int j = 0; j < cand.Length; j++)
            {
                if (cand[j].Length != 2) continue;
                for (int k = j + 1; k < cand.Length; k++)
                {
                    if (cand[j].SequenceEqual(cand[k])) // Two cells in a block have the same candidates
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            if (j == i || k == i) continue; // Don't blacklist in our pair's cells
                            if (BlacklistCandidates(new Point[] { region.Points[i] }, cand[j])) changed = true;
                        }
                        if (changed) Log("Naked pair", "{0}: {1}", new Point[] { region.Points[j], region.Points[k] }.Print(), cand[j].Print());
                    }
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
                foreach (int b in cand)
                {
                    if (candidates[p.X][p.Y][b - 1] != 0)
                    {
                        changed = true;
                        candidates[p.X][p.Y][b - 1] = 0;
                        if (!specials.ContainsKey(p)) specials.Add(p, new List<int>());
                        specials[p].Add(b);
                    }
                }
            }
            return changed;
        }
    }
}
