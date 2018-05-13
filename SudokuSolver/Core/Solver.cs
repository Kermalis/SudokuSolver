using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SudokuSolver.Core
{
    public class Solver
    {
        public readonly Puzzle Puzzle;

        string[] fishStr = new string[] { "", "", "X-Wing", "Swordfish", "Jellyfish" };
        string[] tupleStr = new string[] { "", "single", "pair", "triple", "quadruple" };

        public Solver(int[][] inBoard, bool bCustom) => Puzzle = new Puzzle(inBoard, bCustom);

        public void DoWork(object sender, DoWorkEventArgs e)
        {
            Puzzle.RefreshCandidates();
            Puzzle.Log("Begin");
            bool changed, solved;

            do
            {
                changed = false; // If this is true at the end of the loop, loop again
                solved = true; // If this is true after a segment, the puzzle is solved and we can break

                // Check for naked singles or a completed puzzle
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        SPoint p = new SPoint(x, y);
                        if (Puzzle[p] != 0) continue;

                        solved = false;
                        // Check for naked singles
                        var a = Puzzle[p].Candidates.ToArray();
                        if (a.Length == 1)
                        {
                            Puzzle[p].Set(a[0]);
                            Puzzle.Log("Naked single", new SPoint[] { p }, a);
                            changed = true;
                        }
                    }
                }
                if (solved) break;
                if (changed) continue;

                // Check for hidden singles
                for (int i = 0; i < 9; i++)
                {
                    foreach (Region[] r in Puzzle.Regions)
                    {
                        for (int v = 1; v <= 9; v++)
                        {
                            SPoint[] p = r[i].GetPointsWithCandidates(v);
                            if (p.Length == 1)
                            {
                                Puzzle[p[0]].Set(v);
                                Puzzle.Log("Hidden single", p, v);
                                changed = true;
                            }
                        }
                    }
                }
                if (changed) continue; // Do another pass with simple logic before moving onto more intensive logic

                // Check for naked/locked pairs - http://hodoku.sourceforge.net/en/tech_naked.php#n2
                for (int i = 0; i < 9; i++)
                {
                    if (FindNaked(Puzzle.Blocks[i], 2)
                        || FindNaked(Puzzle.Rows[i], 2)
                        || FindNaked(Puzzle.Columns[i], 2)) { changed = true; break; }
                }
                if (changed) continue;

                // Check for hidden pairs - http://hodoku.sourceforge.net/en/tech_hidden.php#h2
                for (int i = 0; i < 9; i++)
                {
                    if (FindHidden(Puzzle.Blocks[i], 2)
                        || FindHidden(Puzzle.Rows[i], 2)
                        || FindHidden(Puzzle.Columns[i], 2)) { changed = true; break; }
                }
                if (changed) continue;

                // Check for locked row/column candidates - http://hodoku.sourceforge.net/en/tech_intersections.php#lc1
                for (int i = 0; i < 9; i++)
                {
                    for (int v = 1; v <= 9; v++)
                    {
                        if (FindLocked(true, i, v) || FindLocked(false, i, v)) changed = true;
                    }
                }
                if (changed) continue;

                // Check for Y-Wings - http://www.sudokuwiki.org/Y_Wing_Strategy
                for (int i = 0; i < 9; i++)
                {
                    if (FindYWing(Puzzle.Rows[i]) || FindYWing(Puzzle.Columns[i])) { changed = true; break; }
                }
                if (changed) continue;

                // Check for XYZ-Wings - http://www.sudokuwiki.org/XYZ_Wing
                for (int i = 0; i < 9; i++)
                {
                    if (FindXYZWing(Puzzle.Rows[i]) || FindXYZWing(Puzzle.Columns[i])) { changed = true; break; }
                }
                if (changed) continue;

                // Check for pointing pairs/triples - http://hodoku.sourceforge.net/en/tech_intersections.php#lc1
                // I did not make this a dedicated function because the loops would happen more than they already do
                for (int i = 0; i < 3; i++)
                {
                    SPoint[][] blockrow = new SPoint[3][], blockcol = new SPoint[3][];
                    for (int r = 0; r < 3; r++)
                    {
                        blockrow[r] = Puzzle.Blocks[r + (i * 3)].Points;
                        blockcol[r] = Puzzle.Blocks[i + (r * 3)].Points;
                    }
                    for (int r = 0; r < 3; r++) // 3 blocks in a blockrow/blockcolumn
                    {
                        int[][] rowCand = new int[3][], colCand = new int[3][];
                        for (int j = 0; j < 3; j++) // 3 rows/columns in block
                        {
                            // The 3 cells' candidates in a block's row/column
                            rowCand[j] = blockrow[r].GetRow(j).Select(p => Puzzle[p].Candidates).UniteAll().ToArray();
                            colCand[j] = blockcol[r].GetColumn(j).Select(p => Puzzle[p].Candidates).UniteAll().ToArray();
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

                // Check for naked/locked triples - http://hodoku.sourceforge.net/en/tech_naked.php#n3
                for (int i = 0; i < 9; i++)
                {
                    if (FindNaked(Puzzle.Blocks[i], 3)
                        || FindNaked(Puzzle.Rows[i], 3)
                        || FindNaked(Puzzle.Columns[i], 3)) { changed = true; break; }
                }
                if (changed) continue;

                // Check for hidden triples - http://hodoku.sourceforge.net/en/tech_hidden.php#h3
                for (int i = 0; i < 9; i++)
                {
                    if (FindHidden(Puzzle.Blocks[i], 3)
                        || FindHidden(Puzzle.Rows[i], 3)
                        || FindHidden(Puzzle.Columns[i], 3)) { changed = true; break; }
                }
                if (changed) continue;

                // Check for X-Wings, Swordfish & Jellyfish - http://hodoku.sourceforge.net/en/tech_fishb.php
                if (FindFish(2) || FindFish(3) || FindFish(4)) { changed = true; continue; }

                // Check for naked quads - http://hodoku.sourceforge.net/en/tech_naked.php#n4
                for (int i = 0; i < 9; i++)
                {
                    if (FindNaked(Puzzle.Blocks[i], 4)
                        || FindNaked(Puzzle.Rows[i], 4)
                        || FindNaked(Puzzle.Columns[i], 4)) { changed = true; break; }
                }
                if (changed) continue;

                // Check for hidden quads - http://hodoku.sourceforge.net/en/tech_hidden.php#h2
                for (int i = 0; i < 9; i++)
                {
                    if (FindHidden(Puzzle.Blocks[i], 4)
                        || FindHidden(Puzzle.Rows[i], 4)
                        || FindHidden(Puzzle.Columns[i], 4)) { changed = true; break; }
                }

                // Check for unique rectangles - http://hodoku.sourceforge.net/en/tech_ur.php
                if (FindUniqueRectangles()) { changed = true; continue; }

            } while (changed);

            e.Result = solved;
        }

        bool FindUniqueRectangles()
        {
            for (int t = 1; t <= 6; t++) // Type
            {
                for (int x = 0; x < 9; x++)
                {
                    var c1 = Puzzle.Columns[x];
                    for (int x2 = x + 1; x2 < 9; x2++)
                    {
                        var c2 = Puzzle.Columns[x2];
                        for (int y = 0; y < 9; y++)
                        {
                            for (int y2 = y + 1; y2 < 9; y2++)
                            {
                                for (int v = 1; v <= 9; v++)
                                {
                                    for (int v2 = v + 1; v2 <= 9; v2++)
                                    {
                                        var cand = new int[] { v, v2 };
                                        var cells = new Cell[] { c1.Cells[y], c1.Cells[y2], c2.Cells[y], c2.Cells[y2] };
                                        if (cells.Any(c => !c.Candidates.ContainsAll(cand))) continue;

                                        var l = cells.ToLookup(c => c.Candidates.Count);
                                        var gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g).ToArray();

                                        switch (t) // Check for candidate counts
                                        {
                                            case 1: if (l[2].Count() != 3 || gtTwo.Length != 1) continue; break;
                                            case 2: case 6: if (l[2].Count() != 2 || l[3].Count() != 2) continue; break;
                                            case 3: if (l[2].Count() != 2 || gtTwo.Length != 2) continue; break;
                                            case 4: if (l[2].Count() != 2 || l[3].Count() != 1 || l[4].Count() != 1) continue; break;
                                            case 5: if (l[2].Count() != 1 || l[3].Count() != 3) continue; break;
                                        }

                                        Cell[] two = l[2].ToArray(), three = l[3].ToArray(), four = l[4].ToArray();
                                        switch (t) // Check for extra rules
                                        {
                                            case 1:
                                                if (gtTwo[0].Candidates.Count == 3) gtTwo[0].Set(gtTwo[0].Candidates.Single(c => !cand.Contains(c)));
                                                else Puzzle.ChangeCandidates(gtTwo, cand);
                                                break;
                                            case 2:
                                                if (!three[0].Candidates.SetEquals(three[1].Candidates)) continue;
                                                if (!Puzzle.ChangeCandidates(three[0].GetCanSeePoints().Intersect(three[1].GetCanSeePoints()), three[0].Candidates.Except(cand))) continue;
                                                break;
                                            case 3:
                                                if (gtTwo[0].Point.X != gtTwo[1].Point.X && gtTwo[0].Point.Y != gtTwo[1].Point.Y) continue; // Must be non-diagonal
                                                var others = gtTwo[0].Candidates.Except(cand).Union(gtTwo[1].Candidates.Except(cand));
                                                if (others.Count() > 4 || others.Count() < 2) continue;
                                                IEnumerable<Cell> nSubset = ((gtTwo[0].Point.Y == gtTwo[1].Point.Y) ? // Same row
                                                    Puzzle.Rows[gtTwo[0].Point.Y] : Puzzle.Columns[gtTwo[0].Point.X])
                                                    .Cells.Where(c => c.Candidates.ContainsAny(others) && !c.Candidates.ContainsAny(Enumerable.Range(1, 9).Except(others)));
                                                if (nSubset.Count() != others.Count() - 1) continue;
                                                if (!Puzzle.ChangeCandidates(nSubset.Union(gtTwo).Select(c => c.GetCanSee()).IntersectAll(), others)) continue;
                                                break;
                                            case 4:
                                                var remove = new int[1];
                                                if (four[0].Block == three[0].Block)
                                                {
                                                    if (Puzzle.Blocks[four[0].Block].GetCellsWithCandidates(v).Length == 2)
                                                        remove[0] = v2;
                                                    else if (Puzzle.Blocks[four[0].Block].GetCellsWithCandidates(v2).Length == 2)
                                                        remove[0] = v;
                                                }
                                                if (remove[0] != 0) // They share the same row/column but not the same block
                                                {
                                                    if (three[0].Point.X == three[0].Point.X)
                                                    {
                                                        if (Puzzle.Columns[four[0].Point.X].GetCellsWithCandidates(v).Length == 2)
                                                            remove[0] = v2;
                                                        else if (Puzzle.Columns[four[0].Point.X].GetCellsWithCandidates(v2).Length == 2)
                                                            remove[0] = v;
                                                    }
                                                    else
                                                    {
                                                        if (Puzzle.Rows[four[0].Point.Y].GetCellsWithCandidates(v).Length == 2)
                                                            remove[0] = v2;
                                                        else if (Puzzle.Rows[four[0].Point.Y].GetCellsWithCandidates(v2).Length == 2)
                                                            remove[0] = v;
                                                    }
                                                }
                                                else continue;
                                                Puzzle.ChangeCandidates(cells.Except(l[2]), remove);
                                                break;
                                            case 5:
                                                if (!three[0].Candidates.SetEquals(three[1].Candidates) || !three[1].Candidates.SetEquals(three[2].Candidates)) continue;
                                                if (!Puzzle.ChangeCandidates(three.Select(c => c.GetCanSeePoints()).IntersectAll(), three[0].Candidates.Except(cand))) continue;
                                                break;
                                            case 6:
                                                if (three[0].Point.X == three[1].Point.X) continue;
                                                int set = 0;
                                                if (c1.GetCellsWithCandidates(v).Length == 2 && c2.GetCellsWithCandidates(v).Length == 2 // Check if "v" only appears in the UR
                                                    && Puzzle.Rows[two[0].Point.Y].GetCellsWithCandidates(v).Length == 2
                                                        && Puzzle.Rows[two[1].Point.Y].GetCellsWithCandidates(v).Length == 2)
                                                    set = v;
                                                else if (c1.GetCellsWithCandidates(v2).Length == 2 && c2.GetCellsWithCandidates(v2).Length == 2
                                                    && Puzzle.Rows[two[0].Point.Y].GetCellsWithCandidates(v2).Length == 2
                                                        && Puzzle.Rows[two[1].Point.Y].GetCellsWithCandidates(v2).Length == 2)
                                                    set = v2;
                                                else continue;
                                                two[0].Set(set);
                                                two[1].Set(set);
                                                break;
                                        }

                                        // Found UR
                                        Puzzle.Log("Unique Rectangle", cells, cand);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        bool FindXYZWing(Region region)
        {
            bool changed = false;
            var points2 = region.Points.Where(p => Puzzle[p].Candidates.Count == 2).ToArray();
            var points3 = region.Points.Where(p => Puzzle[p].Candidates.Count == 3).ToArray();
            if (points2.Length > 0 && points3.Length > 0)
            {
                for (int i = 0; i < points2.Length; i++)
                {
                    SPoint p2 = points2[i];
                    for (int j = 0; j < points3.Length; j++)
                    {
                        SPoint p3 = points3[j];
                        if (Puzzle[p2].Candidates.Intersect(Puzzle[p3].Candidates).Count() != 2) continue;

                        var p3Sees = Puzzle[p3].GetCanSeePoints().Except(region.Points)
                            .Where(p => Puzzle[p].Candidates.Count == 2 // If it has 2 candidates
                            && Puzzle[p].Candidates.Intersect(Puzzle[p3].Candidates).Count() == 2 // Shares them both with p3
                            && Puzzle[p].Candidates.Intersect(Puzzle[p2].Candidates).Count() == 1); // And shares one with p2
                        foreach (SPoint p2_2 in p3Sees)
                        {
                            var allSee = Puzzle[p2].GetCanSeePoints().Intersect(Puzzle[p3].GetCanSeePoints()).Intersect(Puzzle[p2_2].GetCanSeePoints());
                            var allHave = Puzzle[p2].Candidates.Intersect(Puzzle[p3].Candidates).Intersect(Puzzle[p2_2].Candidates).ToArray(); // Will be 1 Length
                            if (Puzzle.ChangeCandidates(allSee, allHave))
                            {
                                changed = true;
                                var culprits = new SPoint[] { p2, p3, p2_2 };
                                Puzzle.Log("XYZ-Wing", culprits, allHave);
                            }
                        }
                    }
                }
            }
            return changed;
        }

        bool FindYWing(Region region)
        {
            var points = region.Points.Where(p => Puzzle[p].Candidates.Count == 2).ToArray();
            if (points.Length > 1)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    SPoint p1 = points[i];
                    for (int j = i + 1; j < points.Length; j++)
                    {
                        SPoint p2 = points[j];
                        var inter = Puzzle[p1].Candidates.Intersect(Puzzle[p2].Candidates).ToArray();
                        if (inter.Length != 1) continue;

                        var a = new int[] { inter[0] };
                        int other1 = Puzzle[p1].Candidates.Except(a).ToArray()[0],
                            other2 = Puzzle[p2].Candidates.Except(a).ToArray()[0];

                        var b = new SPoint[] { p1, p2 };
                        foreach (SPoint point in b)
                        {
                            var p3a = Puzzle[point].GetCanSeePoints().Except(points).Where(p => Puzzle[p].Candidates.Count == 2 && Puzzle[p].Candidates.Intersect(new int[] { other1, other2 }).Count() == 2).ToArray();
                            if (p3a.Length == 1) // Example: p1 and p3 see each other, so remove similarities from p2 and p3
                            {
                                SPoint p3 = p3a[0];
                                SPoint pOther = b.Single(p => p != point);
                                var common = Puzzle[pOther].GetCanSeePoints().Intersect(Puzzle[p3].GetCanSeePoints());
                                var cand = Puzzle[pOther].Candidates.Intersect(Puzzle[p3].Candidates).ToArray(); // Will just be 1 candidate
                                if (Puzzle.ChangeCandidates(common, cand))
                                {
                                    var culprits = new SPoint[] { p1, p2, p3 };
                                    Puzzle.Log("Y-Wing", culprits, cand);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        // Find X-Wing, Swordfish & Jellyfish
        bool FindFish(int amt)
        {
            for (int v = 1; v <= 9; v++)
            {
                if (DoFish(v, 0, amt, new int[amt])) return true;
            }
            return false;
        }
        bool DoFish(int cand, int loop, int amt, int[] indexes)
        {
            if (loop == amt)
            {
                SPoint[][] rowPoints = indexes.Select(i => Puzzle.Rows[i].GetPointsWithCandidates(cand)).ToArray(),
                    colPoints = indexes.Select(i => Puzzle.Columns[i].GetPointsWithCandidates(cand)).ToArray();

                IEnumerable<int> rowLengths = rowPoints.Select(parr => parr.Length),
                    colLengths = colPoints.Select(parr => parr.Length);
                
                if (rowLengths.Max() == amt && rowLengths.Min() > 0 && rowPoints.Select(parr => parr.Select(p => p.X)).UniteAll().Count() <= amt)
                {
                    var row2D = rowPoints.UniteAll();
                    if (Puzzle.ChangeCandidates(row2D.Select(p => Puzzle.Columns[p.X].Points).UniteAll().Except(row2D), new int[] { cand }))
                    {
                        Puzzle.Log(fishStr[amt], row2D, cand);
                        return true;
                    }
                }
                if (colLengths.Max() == amt && colLengths.Min() > 0 && colPoints.Select(parr => parr.Select(p => p.Y)).UniteAll().Count() <= amt)
                {
                    var col2D = colPoints.UniteAll();
                    if (Puzzle.ChangeCandidates(col2D.Select(p => Puzzle.Rows[p.Y].Points).UniteAll().Except(col2D), new int[] { cand }))
                    {
                        Puzzle.Log(fishStr[amt], col2D, cand);
                        return true;
                    }
                }
            }
            else
            {
                for (int i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
                {
                    indexes[loop] = i;
                    if (DoFish(cand, loop + 1, amt, indexes)) return true;
                }
            }
            return false;
        }

        bool FindLocked(bool doRows, int rc, int value)
        {
            var with = (doRows ? Puzzle.Rows : Puzzle.Columns)[rc].GetPointsWithCandidates(value);

            // Even if a block only has these candidates for this "k" value, it'd be slower to check that before cancelling "BlacklistCandidates"
            if (with.Count() == 3 || with.Count() == 2)
            {
                var blocks = with.Select(p => Puzzle[p].Block).Distinct().ToArray();
                if (blocks.Length == 1)
                    if (Puzzle.ChangeCandidates(Puzzle.Blocks[blocks[0]].Points.Except(with), new int[] { value }))
                    {
                        Puzzle.Log("Locked candidate", with, "{4} {0} locks within block {1}: {2}: {3}", doRows ? SPoint.RowL(rc) : (rc + 1).ToString(), blocks[0] + 1, with.Print(), value, doRows ? "Row" : "Column");
                        return true;
                    }
            }
            return false;
        }

        // Find hidden pairs/triples/quadruples
        bool FindHidden(Region region, int amt)
        {
            if (region.Points.Count(p => Puzzle[p].Candidates.Count > 0) == amt) // If there are only "amt" cells with candidates, we don't have to waste our time
                return false;
            return DoHidden(region, 0, amt, new int[amt]);
        }
        bool DoHidden(Region region, int loop, int amt, int[] cand)
        {
            if (loop == amt)
            {
                var points = cand.Select(c => region.GetPointsWithCandidates(c)).UniteAll().ToArray();
                var cands = points.Select(p => Puzzle[p].Candidates).UniteAll();
                if (points.Length != amt // There aren't "amt" cells for our tuple to be in
                    || cands.Count() == amt // We already know it's a tuple (might be faster to skip this check, idk)
                    || !cands.ContainsAll(cand)) return false; // If a number in our combo doesn't actually show up in any of our cells
                if (Puzzle.ChangeCandidates(points, Enumerable.Range(1, 9).Except(cand)))
                {
                    Puzzle.Log("Hidden " + tupleStr[amt], points, cand);
                    return true;
                }
            }
            else
            {
                for (int i = cand[loop == 0 ? loop : loop - 1] + 1; i <= 9; i++)
                {
                    cand[loop] = i;
                    if (DoHidden(region, loop + 1, amt, cand)) return true;
                }
            }
            return false;
        }

        // Find naked pairs/triples/quadruples
        bool FindNaked(Region region, int amt)
        {
            return DoNaked(region, 0, amt, new Cell[amt], new int[amt]);
        }
        bool DoNaked(Region region, int loop, int amt, Cell[] cells, int[] indexes)
        {
            if (loop == amt)
            {
                var combo = cells.Select(c => c.Candidates).UniteAll().ToArray();
                if (combo.Length == amt)
                {
                    if (Puzzle.ChangeCandidates(indexes.Select(i => Puzzle[region.Points[i]].GetCanSeePoints()).IntersectAll(), combo))
                    {
                        Puzzle.Log("Naked " + tupleStr[amt], cells, combo);
                        return true;
                    }
                }
            }
            else
            {
                for (int i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
                {
                    Cell c = region.Cells[i];
                    if (c.Candidates.Count == 0) continue;
                    cells[loop] = c;
                    indexes[loop] = i;
                    if (DoNaked(region, loop + 1, amt, cells, indexes)) return true;
                }
            }
            return false;
        }

        // Clear candidates from a blockrow/blockcolumn and return true if something changed
        bool RemoveBlockRowColCandidates(SPoint[][] blockrcs, bool doRows, int current, int ignoreBlock, int rc, IEnumerable<int> cand)
        {
            bool changed = false;
            for (int i = 0; i < 3; i++)
            {
                if (i == ignoreBlock) continue;
                var rcs = doRows ? blockrcs[i].GetRow(rc) : blockrcs[i].GetColumn(rc);
                if (Puzzle.ChangeCandidates(rcs, cand)) changed = true;
            }
            if (changed) Puzzle.Log("Pointing couple", doRows ? blockrcs[ignoreBlock].GetRow(rc) : blockrcs[ignoreBlock].GetColumn(rc),
                "Starting in block{0} {1}'s block {2}, {0} {3}: {4}", doRows ? "row" : "column", current + 1, ignoreBlock + 1, doRows ? SPoint.RowL(rc) : (rc + 1).ToString(), cand.Print());
            return changed;
        }
    }
}
