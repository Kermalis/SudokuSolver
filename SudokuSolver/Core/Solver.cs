using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using SudokuSolver.Core;

namespace Kermalis.SudokuSolver.Core
{
    internal class Solver
    {
        private static readonly SolverTechnique[] techniques =
        {
            new SolverTechnique(HiddenSingle, "Hidden single", true),
            new SolverTechnique(NakedPair, "http://hodoku.sourceforge.net/en/tech_naked.php#n2"),
            new SolverTechnique(HiddenPair, "http://hodoku.sourceforge.net/en/tech_hidden.php#h2"),
            new SolverTechnique(LockedCandidate, "http://hodoku.sourceforge.net/en/tech_intersections.php#lc1"),
            new SolverTechnique(PointingTuple, "http://hodoku.sourceforge.net/en/tech_intersections.php#lc1"),
            new SolverTechnique(NakedTriple, "http://hodoku.sourceforge.net/en/tech_naked.php#n3"),
            new SolverTechnique(HiddenTriple, "http://hodoku.sourceforge.net/en/tech_hidden.php#h3"),
            new SolverTechnique(XWing, "http://hodoku.sourceforge.net/en/tech_fishb.php#bf2"),
            new SolverTechnique(Swordfish, "http://hodoku.sourceforge.net/en/tech_fishb.php#bf3"),
            new SolverTechnique(YWing, "http://www.sudokuwiki.org/Y_Wing_Strategy"),
            new SolverTechnique(XYZWing, "http://www.sudokuwiki.org/XYZ_Wing"),
            new SolverTechnique(NakedQuadruple, "http://hodoku.sourceforge.net/en/tech_naked.php#n4"),
            new SolverTechnique(HiddenQuadruple, "http://hodoku.sourceforge.net/en/tech_hidden.php#h4"),
            new SolverTechnique(Jellyfish, "http://hodoku.sourceforge.net/en/tech_fishb.php#bf4"),
            new SolverTechnique(UniqueRectangle, "http://hodoku.sourceforge.net/en/tech_ur.php"),
            new SolverTechnique(HiddenRectangle, "http://hodoku.sourceforge.net/en/tech_ur.php#hr"),
            new SolverTechnique(AvoidableRectangle, "http://hodoku.sourceforge.net/en/tech_ur.php#ar")
        };

        private SolverTechnique _previousTechnique;

        public Solver(Puzzle puzzle)
        {
            Puzzle = puzzle;
        }

       
        public Puzzle Puzzle { get;  }

        public void DoWork(object sender, DoWorkEventArgs e)
        {
            Puzzle.RefreshCandidates();
            Puzzle.LogAction("Begin");

            bool solved; // If this is true after a segment, the puzzle is solved and we can break
            do
            {
                solved = true;

                var changed = false;
                // Check for naked singles or a completed puzzle
                for (var x = 0; x < 9; x++)
                for (var y = 0; y < 9; y++)
                {
                    var cell = Puzzle[x, y];
                    if (cell.Value == 0)
                    {
                        solved = false;
                        // Check for naked singles
                        var a = cell.Candidates.ToArray(); // Copy
                        if (a.Length == 1)
                        {
                            cell.Set(a[0]);
                            Puzzle.LogAction("Naked single", new[] {cell}, a);
                            changed = true;
                        }
                    }
                }

                // Solved or failed to solve
                if (solved || !changed && !RunTechnique()) break;
            } while (true);

            if (!solved)
            {
                SearchStrategy search = new SearchStrategy();
                solved = search.SolvePuzzle(Puzzle);
                foreach (Region resultRow in search.Result.Rows)
                {
                    foreach (Cell resultRowCell in resultRow.Cells)
                    {
                        var pointX = resultRowCell.Point.X;
                        var pointY = resultRowCell.Point.Y;
                        if (Puzzle[pointX, pointY].Value == 0)
                            Puzzle[pointX, pointY].Set(search.Result[pointX, pointY].Value);
                    }
                }

            }

            e.Result = solved;
        }

        private bool RunTechnique()
        {
            foreach (var t in techniques)
                // Skip the previous technique unless it is good to repeat it
                if ((t != _previousTechnique || t.CanRepeat) && t.Function.Invoke(Puzzle))
                {
                    _previousTechnique = t;
                    return true;
                }

            if (_previousTechnique == null)
                return false;
            return _previousTechnique.Function.Invoke(Puzzle);
        }

        private class SolverTechnique
        {
            public SolverTechnique(Func<Puzzle, bool> function, string url, bool canRepeat = false)
            {
                // I know the text is unused
                Function = function;
                CanRepeat = canRepeat;
            }

            public Func<Puzzle, bool> Function { get; }
            public bool CanRepeat { get; }
        }


        #region Methods

        private static bool AvoidableRectangle(Puzzle puzzle)
        {
            for (var type = 1; type <= 2; type++)
            for (var x1 = 0; x1 < 9; x1++)
            {
                var c1 = puzzle.Columns[x1];
                for (var x2 = x1 + 1; x2 < 9; x2++)
                {
                    var c2 = puzzle.Columns[x2];
                    for (var y1 = 0; y1 < 9; y1++)
                    for (var y2 = y1 + 1; y2 < 9; y2++)
                    for (var value1 = 1; value1 <= 9; value1++)
                    for (var value2 = value1 + 1; value2 <= 9; value2++)
                    {
                        int[] candidates = {value1, value2};
                        Cell[] cells = {c1.Cells[y1], c1.Cells[y2], c2.Cells[y1], c2.Cells[y2]};
                        if (cells.Any(c => c.OriginalValue != 0)) continue;

                        IEnumerable<Cell> alreadySet = cells.Where(c => c.Value != 0),
                            notSet = cells.Where(c => c.Value == 0);

                        switch (type)
                        {
                            case 1:
                            {
                                if (alreadySet.Count() != 3) continue;
                                break;
                            }
                            case 2:
                            {
                                if (alreadySet.Count() != 2) continue;
                                break;
                            }
                        }

                        var no = true;
                        Cell[][] pairs = {new[] {cells[0], cells[3]}, new[] {cells[1], cells[2]}};
                        foreach (var pair in pairs)
                        foreach (var i in candidates)
                        {
                            var otherPair = pairs.Single(carr => carr != pair);
                            var otherVal = candidates.Single(ca => ca != i);
                            if ((pair[0].Value == i && pair[1].Value == 0 && pair[1].Candidates.Count == 2 &&
                                 pair[1].Candidates.Contains(i)
                                 || pair[1].Value == i && pair[0].Value == 0 && pair[0].Candidates.Count == 2 &&
                                 pair[0].Candidates.Contains(i))
                                && otherPair.All(c =>
                                    c.Value == otherVal || c.Candidates.Count == 2 && c.Candidates.Contains(otherVal)))
                                no = false;
                        }

                        if (no) continue;
                        var changed = false;
                        switch (type)
                        {
                            case 1:
                            {
                                var cell = notSet.ElementAt(0);
                                if (cell.Candidates.Count == 2)
                                    cell.Set(cell.Candidates.Except(candidates).ElementAt(0));
                                else
                                    puzzle.RemoveCandidates(new[] {cell}, cell.Candidates.Intersect(candidates));
                                changed = true;
                                break;
                            }
                            case 2:
                            {
                                var commonCandidates =
                                    notSet.Select(c => c.Candidates.Except(candidates)).IntersectAll();
                                if (commonCandidates.Any()
                                    && puzzle.RemoveCandidates(notSet.Select(c => c.GetCellsVisible()).IntersectAll(),
                                        commonCandidates))
                                    changed = true;
                                break;
                            }
                        }

                        if (changed)
                        {
                            puzzle.LogAction("Avoidable rectangle", cells, candidates);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool HiddenRectangle(Puzzle puzzle)
        {
            for (var x1 = 0; x1 < 9; x1++)
            {
                var c1 = puzzle.Columns[x1];
                for (var x2 = x1 + 1; x2 < 9; x2++)
                {
                    var c2 = puzzle.Columns[x2];
                    for (var y1 = 0; y1 < 9; y1++)
                    for (var y2 = y1 + 1; y2 < 9; y2++)
                    for (var value1 = 1; value1 <= 9; value1++)
                    for (var value2 = value1 + 1; value2 <= 9; value2++)
                    {
                        int[] candidates = {value1, value2};
                        Cell[] cells = {c1.Cells[y1], c1.Cells[y2], c2.Cells[y1], c2.Cells[y2]};
                        if (cells.Any(c => !c.Candidates.ContainsAll(candidates))) continue;
                        var l = cells.ToLookup(c => c.Candidates.Count);
                        var gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g);
                        var gtTwoCount = gtTwo.Count();
                        if (gtTwoCount < 2 || gtTwoCount > 3) continue;

                        var changed = false;
                        foreach (var c in l[2])
                        {
                            int eks = c.Point.X == x1 ? x2 : x1,
                                why = c.Point.Y == y1 ? y2 : y1;
                            foreach (var i in candidates)
                                if (!puzzle.Rows[why].GetCellsWithCandidates(i).Except(cells)
                                        .Any() // "i" only appears in our UR
                                    && !puzzle.Columns[eks].GetCellsWithCandidates(i).Except(cells).Any())
                                {
                                    var diag = puzzle[eks, why];
                                    if (diag.Candidates.Count == 2)
                                        diag.Set(i);
                                    else
                                        puzzle.RemoveCandidates(new[] {diag}, new[] {i == value1 ? value2 : value1});
                                    changed = true;
                                }
                        }

                        if (changed)
                        {
                            puzzle.LogAction("Hidden rectangle", cells, candidates);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool UniqueRectangle(Puzzle puzzle)
        {
            for (var type = 1; type <= 6; type++) // Type
            for (var x1 = 0; x1 < 9; x1++)
            {
                var c1 = puzzle.Columns[x1];
                for (var x2 = x1 + 1; x2 < 9; x2++)
                {
                    var c2 = puzzle.Columns[x2];
                    for (var y1 = 0; y1 < 9; y1++)
                    for (var y2 = y1 + 1; y2 < 9; y2++)
                    for (var value1 = 1; value1 <= 9; value1++)
                    for (var value2 = value1 + 1; value2 <= 9; value2++)
                    {
                        int[] candidates = {value1, value2};
                        Cell[] cells = {c1.Cells[y1], c1.Cells[y2], c2.Cells[y1], c2.Cells[y2]};
                        if (cells.Any(c => !c.Candidates.ContainsAll(candidates))) continue;

                        var l = cells.ToLookup(c => c.Candidates.Count);
                        Cell[] gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g).ToArray(),
                            two = l[2].ToArray(),
                            three = l[3].ToArray(),
                            four = l[4].ToArray();

                        switch (type) // Check for candidate counts
                        {
                            case 1:
                            {
                                if (two.Length != 3 || gtTwo.Length != 1) continue;
                                break;
                            }
                            case 2:
                            case 6:
                            {
                                if (two.Length != 2 || three.Length != 2) continue;
                                break;
                            }
                            case 3:
                            {
                                if (two.Length != 2 || gtTwo.Length != 2) continue;
                                break;
                            }
                            case 4:
                            {
                                if (two.Length != 2 || three.Length != 1 || four.Length != 1) continue;
                                break;
                            }
                            case 5:
                            {
                                if (two.Length != 1 || three.Length != 3) continue;
                                break;
                            }
                        }

                        switch (type) // Check for extra rules
                        {
                            case 1:
                            {
                                if (gtTwo[0].Candidates.Count == 3)
                                    gtTwo[0].Set(gtTwo[0].Candidates.Single(c => !candidates.Contains(c)));
                                else
                                    puzzle.RemoveCandidates(gtTwo, candidates);
                                break;
                            }
                            case 2:
                            {
                                if (!three[0].Candidates.SetEquals(three[1].Candidates)) continue;
                                if (!puzzle.RemoveCandidates(
                                    three[0].GetCellsVisible().Intersect(three[1].GetCellsVisible()),
                                    three[0].Candidates.Except(candidates))) continue;
                                break;
                            }
                            case 3:
                            {
                                if (gtTwo[0].Point.X != gtTwo[1].Point.X && gtTwo[0].Point.Y != gtTwo[1].Point.Y)
                                    continue; // Must be non-diagonal
                                var others = gtTwo[0].Candidates.Except(candidates)
                                    .Union(gtTwo[1].Candidates.Except(candidates));
                                if (others.Count() > 4 || others.Count() < 2) continue;
                                var nSubset = (gtTwo[0].Point.Y == gtTwo[1].Point.Y
                                        ? // Same row
                                        puzzle.Rows[gtTwo[0].Point.Y]
                                        : puzzle.Columns[gtTwo[0].Point.X])
                                    .Cells.Where(c =>
                                        c.Candidates.ContainsAny(others) &&
                                        !c.Candidates.ContainsAny(Utils.OneToNine.Except(others)));
                                if (nSubset.Count() != others.Count() - 1) continue;
                                if (!puzzle.RemoveCandidates(
                                    nSubset.Union(gtTwo).Select(c => c.GetCellsVisible()).IntersectAll(), others))
                                    continue;
                                break;
                            }
                            case 4:
                            {
                                var remove = new int[1];
                                if (four[0].BlockIndex == three[0].BlockIndex)
                                {
                                    if (puzzle.Blocks[four[0].BlockIndex].GetCellsWithCandidates(value1).Count() == 2)
                                        remove[0] = value2;
                                    else if (puzzle.Blocks[four[0].BlockIndex].GetCellsWithCandidates(value2).Count() ==
                                             2) remove[0] = value1;
                                }

                                if (remove[0] != 0) // They share the same row/column but not the same block
                                {
                                    if (three[0].Point.X == three[0].Point.X)
                                    {
                                        if (puzzle.Columns[four[0].Point.X].GetCellsWithCandidates(value1).Count() == 2)
                                            remove[0] = value2;
                                        else if (puzzle.Columns[four[0].Point.X].GetCellsWithCandidates(value2)
                                                     .Count() == 2) remove[0] = value1;
                                    }
                                    else
                                    {
                                        if (puzzle.Rows[four[0].Point.Y].GetCellsWithCandidates(value1).Count() == 2)
                                            remove[0] = value2;
                                        else if (puzzle.Rows[four[0].Point.Y].GetCellsWithCandidates(value2).Count() ==
                                                 2) remove[0] = value1;
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                puzzle.RemoveCandidates(cells.Except(l[2]), remove);
                                break;
                            }
                            case 5:
                            {
                                if (!three[0].Candidates.SetEquals(three[1].Candidates) ||
                                    !three[1].Candidates.SetEquals(three[2].Candidates)) continue;
                                if (!puzzle.RemoveCandidates(three.Select(c => c.GetCellsVisible()).IntersectAll(),
                                    three[0].Candidates.Except(candidates))) continue;
                                break;
                            }
                            case 6:
                            {
                                if (three[0].Point.X == three[1].Point.X) continue;
                                int set;
                                if (c1.GetCellsWithCandidates(value1).Count() == 2 && c2.GetCellsWithCandidates(value1)
                                                                                       .Count() ==
                                                                                   2 // Check if "v" only appears in the UR
                                                                                   && puzzle.Rows[two[0].Point.Y]
                                                                                       .GetCellsWithCandidates(value1)
                                                                                       .Count() == 2
                                                                                   && puzzle.Rows[two[1].Point.Y]
                                                                                       .GetCellsWithCandidates(value1)
                                                                                       .Count() == 2)
                                    set = value1;
                                else if (c1.GetCellsWithCandidates(value2).Count() == 2 && c2.GetCellsWithCandidates(
                                                                                            value2).Count() == 2
                                                                                        && puzzle.Rows[two[0].Point.Y]
                                                                                            .GetCellsWithCandidates(
                                                                                                value2).Count() == 2
                                                                                        && puzzle.Rows[two[1].Point.Y]
                                                                                            .GetCellsWithCandidates(
                                                                                                value2).Count() == 2)
                                    set = value2;
                                else
                                    continue;
                                two[0].Set(set);
                                two[1].Set(set);
                                break;
                            }
                        }

                        puzzle.LogAction("Unique rectangle", cells, candidates);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool XYZWing(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
            {
                bool FindXYZWings(Region region)
                {
                    var changed = false;
                    var cells2 = region.Cells.Where(c => c.Candidates.Count == 2).ToArray();
                    var cells3 = region.Cells.Where(c => c.Candidates.Count == 3).ToArray();
                    if (cells2.Length > 0 && cells3.Length > 0)
                        for (var j = 0; j < cells2.Length; j++)
                        {
                            var c2 = cells2[j];
                            for (var k = 0; k < cells3.Length; k++)
                            {
                                var c3 = cells3[k];
                                if (c2.Candidates.Intersect(c3.Candidates).Count() != 2) continue;

                                var c3Sees = c3.GetCellsVisible().Except(region.Cells)
                                    .Where(c => c.Candidates.Count == 2 // If it has 2 candidates
                                                && c.Candidates.Intersect(c3.Candidates).Count() ==
                                                2 // Shares them both with p3
                                                && c.Candidates.Intersect(c3.Candidates).Count() ==
                                                1); // And shares one with p2
                                foreach (var c2_2 in c3Sees)
                                {
                                    var allSee = c2.GetCellsVisible().Intersect(c3.GetCellsVisible())
                                        .Intersect(c2_2.GetCellsVisible());
                                    var allHave = c2.Candidates.Intersect(c3.Candidates)
                                        .Intersect(c2_2.Candidates); // Will be 1 Length
                                    if (puzzle.RemoveCandidates(allSee, allHave))
                                    {
                                        puzzle.LogAction("XYZ-Wing", new[] {c2, c3, c2_2}, allHave);
                                        changed = true;
                                    }
                                }
                            }
                        }

                    return changed;
                }

                if (FindXYZWings(puzzle.Rows[i]) || FindXYZWings(puzzle.Columns[i])) return true;
            }

            return false;
        }

        private static bool YWing(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
            {
                bool FindYWings(Region region)
                {
                    var cells = region.Cells.Where(c => c.Candidates.Count == 2).ToArray();
                    if (cells.Length > 1)
                        for (var j = 0; j < cells.Length; j++)
                        {
                            var c1 = cells[j];
                            for (var k = j + 1; k < cells.Length; k++)
                            {
                                var c2 = cells[k];
                                var inter = c1.Candidates.Intersect(c2.Candidates);
                                if (inter.Count() != 1) continue;

                                int other1 = c1.Candidates.Except(inter).ElementAt(0),
                                    other2 = c2.Candidates.Except(inter).ElementAt(0);

                                Cell[] a = {c1, c2};
                                foreach (var cell in a)
                                {
                                    var c3a = cell.GetCellsVisible().Except(cells).Where(c =>
                                        c.Candidates.Count == 2 &&
                                        c.Candidates.Intersect(new[] {other1, other2}).Count() == 2);
                                    if (c3a.Count() == 1
                                    ) // Example: p1 and p3 see each other, so remove similarities from p2 and p3
                                    {
                                        var c3 = c3a.ElementAt(0);
                                        var cOther = a.Single(c => c != cell);
                                        var commonCells = cOther.GetCellsVisible().Intersect(c3.GetCellsVisible());
                                        var candidate =
                                            cOther.Candidates.Intersect(c3.Candidates); // Will just be 1 candidate
                                        if (puzzle.RemoveCandidates(commonCells, candidate))
                                        {
                                            puzzle.LogAction("Y-Wing", new[] {c1, c2, c3}, candidate);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }

                    return false;
                }

                if (FindYWings(puzzle.Rows[i]) || FindYWings(puzzle.Columns[i])) return true;
            }

            return false;
        }

        private static bool Jellyfish(Puzzle puzzle)
        {
            return FindFish(puzzle, 4);
        }

        private static bool Swordfish(Puzzle puzzle)
        {
            return FindFish(puzzle, 3);
        }

        private static bool XWing(Puzzle puzzle)
        {
            return FindFish(puzzle, 2);
        }

        private static bool PointingTuple(Puzzle puzzle)
        {
            for (var i = 0; i < 3; i++)
            {
                Cell[][] blockrow = new Cell[3][], blockcol = new Cell[3][];
                for (var r = 0; r < 3; r++)
                {
                    blockrow[r] = puzzle.Blocks[r + i * 3].Cells.ToArray();
                    blockcol[r] = puzzle.Blocks[i + r * 3].Cells.ToArray();
                }

                for (var r = 0; r < 3; r++) // 3 blocks in a blockrow/blockcolumn
                {
                    int[][] rowCandidates = new int[3][], colCand = new int[3][];
                    for (var j = 0; j < 3; j++) // 3 rows/columns in block
                    {
                        // The 3 cells' candidates in a block's row/column
                        rowCandidates[j] = blockrow[r].GetRowInBlock(j).Select(c => c.Candidates).UniteAll().ToArray();
                        colCand[j] = blockcol[r].GetColumnInBlock(j).Select(c => c.Candidates).UniteAll().ToArray();
                    }

                    bool RemovePointingTuple(bool doRows, int rcIndex, IEnumerable<int> candidates)
                    {
                        var changed = false;
                        for (var j = 0; j < 3; j++)
                        {
                            if (j == r) continue;
                            var rcs = doRows
                                ? blockrow[j].GetRowInBlock(rcIndex)
                                : blockcol[j].GetColumnInBlock(rcIndex);
                            if (puzzle.RemoveCandidates(rcs, candidates)) changed = true;
                        }

                        if (changed)
                            puzzle.LogAction("Pointing tuple",
                                doRows ? blockrow[r].GetRowInBlock(rcIndex) : blockcol[r].GetColumnInBlock(rcIndex),
                                "Starting in block{0} {1}'s {2} block, {3} {0}: {4}", doRows ? "row" : "column", i + 1,
                                ordinalStr[r + 1], ordinalStr[rcIndex + 1],
                                candidates.Count() == 1 ? candidates.ElementAt(0).ToString() : candidates.Print());
                        return changed;
                    }

                    // Now check if a row has a distinct candidate
                    var zero_distinct = rowCandidates[0].Except(rowCandidates[1]).Except(rowCandidates[2]);
                    if (zero_distinct.Count() > 0)
                        if (RemovePointingTuple(true, 0, zero_distinct))
                            return true;
                    var one_distinct = rowCandidates[1].Except(rowCandidates[0]).Except(rowCandidates[2]);
                    if (one_distinct.Count() > 0)
                        if (RemovePointingTuple(true, 1, one_distinct))
                            return true;
                    var two_distinct = rowCandidates[2].Except(rowCandidates[0]).Except(rowCandidates[1]);
                    if (two_distinct.Count() > 0)
                        if (RemovePointingTuple(true, 2, two_distinct))
                            return true;
                    // Now check if a column has a distinct candidate
                    zero_distinct = colCand[0].Except(colCand[1]).Except(colCand[2]);
                    if (zero_distinct.Any())
                        if (RemovePointingTuple(false, 0, zero_distinct))
                            return true;
                    one_distinct = colCand[1].Except(colCand[0]).Except(colCand[2]);
                    if (one_distinct.Any())
                        if (RemovePointingTuple(false, 1, one_distinct))
                            return true;
                    two_distinct = colCand[2].Except(colCand[0]).Except(colCand[1]);
                    if (two_distinct.Any())
                        if (RemovePointingTuple(false, 2, two_distinct))
                            return true;
                }
            }

            return false;
        }

        private static bool LockedCandidate(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
            for (var candidate = 1; candidate <= 9; candidate++)
            {
                bool FindLockedCandidates(bool doRows)
                {
                    var cellsWithCandidates =
                        (doRows ? puzzle.Rows : puzzle.Columns)[i].GetCellsWithCandidates(candidate);

                    // Even if a block only has these candidates for this "k" value, it'd be slower to check that before cancelling "BlacklistCandidates"
                    if (cellsWithCandidates.Count() == 3 || cellsWithCandidates.Count() == 2)
                    {
                        var blocks = cellsWithCandidates.Select(c => c.BlockIndex).Distinct().ToArray();
                        if (blocks.Length == 1)
                            if (puzzle.RemoveCandidates(puzzle.Blocks[blocks[0]].Cells.Except(cellsWithCandidates),
                                new[] {candidate}))
                            {
                                puzzle.LogAction("Locked candidate", cellsWithCandidates,
                                    "{4} {0} locks within block {1}: {2}: {3}",
                                    doRows ? SPoint.RowLetter(i) : SPoint.ColumnLetter(i), blocks[0] + 1,
                                    cellsWithCandidates.Print(), candidate, doRows ? "Row" : "Column");
                                return true;
                            }
                    }

                    return false;
                }

                if (FindLockedCandidates(true) || FindLockedCandidates(false)) return true;
            }

            return false;
        }

        private static bool HiddenQuadruple(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
                if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 4)
                    || FindHiddenTuples(puzzle, puzzle.Rows[i], 4)
                    || FindHiddenTuples(puzzle, puzzle.Columns[i], 4))
                    return true;
            return false;
        }

        private static bool NakedQuadruple(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
                if (FindNakedTuples(puzzle, puzzle.Blocks[i], 4)
                    || FindNakedTuples(puzzle, puzzle.Rows[i], 4)
                    || FindNakedTuples(puzzle, puzzle.Columns[i], 4))
                    return true;
            return false;
        }

        private static bool HiddenTriple(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
                if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 3)
                    || FindHiddenTuples(puzzle, puzzle.Rows[i], 3)
                    || FindHiddenTuples(puzzle, puzzle.Columns[i], 3))
                    return true;
            return false;
        }

        private static bool NakedTriple(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
                if (FindNakedTuples(puzzle, puzzle.Blocks[i], 3)
                    || FindNakedTuples(puzzle, puzzle.Rows[i], 3)
                    || FindNakedTuples(puzzle, puzzle.Columns[i], 3))
                    return true;
            return false;
        }

        private static bool HiddenPair(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
                if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 2)
                    || FindHiddenTuples(puzzle, puzzle.Rows[i], 2)
                    || FindHiddenTuples(puzzle, puzzle.Columns[i], 2))
                    return true;
            return false;
        }

        private static bool NakedPair(Puzzle puzzle)
        {
            for (var i = 0; i < 9; i++)
                if (FindNakedTuples(puzzle, puzzle.Blocks[i], 2)
                    || FindNakedTuples(puzzle, puzzle.Rows[i], 2)
                    || FindNakedTuples(puzzle, puzzle.Columns[i], 2))
                    return true;
            return false;
        }

        private static bool HiddenSingle(Puzzle puzzle)
        {
            var changed = false;
            for (var i = 0; i < 9; i++)
                foreach (var region in puzzle.Regions)
                    for (var candidate = 1; candidate <= 9; candidate++)
                    {
                        var c = region[i].GetCellsWithCandidates(candidate).ToArray();
                        if (c.Length == 1)
                        {
                            c[0].Set(candidate);
                            puzzle.LogAction("Hidden single", c, new[] {candidate});
                            changed = true;
                        }
                    }

            return changed;
        }

        #endregion

        #region Method Helpers

        private static readonly string[] fishStr = {string.Empty, string.Empty, "X-Wing", "Swordfish", "Jellyfish"};
        private static readonly string[] tupleStr = {string.Empty, "single", "pair", "triple", "quadruple"};
        private static readonly string[] ordinalStr = {string.Empty, "1st", "2nd", "3rd"};

        // Find X-Wing, Swordfish & Jellyfish
        private static bool FindFish(Puzzle puzzle, int amount)
        {
            for (var candidate = 1; candidate <= 9; candidate++)
            {
                bool DoFish(int loop, int[] indexes)
                {
                    if (loop == amount)
                    {
                        IEnumerable<IEnumerable<Cell>> rowCells =
                                indexes.Select(i => puzzle.Rows[i].GetCellsWithCandidates(candidate)),
                            colCells = indexes.Select(i => puzzle.Columns[i].GetCellsWithCandidates(candidate));

                        IEnumerable<int> rowLengths = rowCells.Select(cells => cells.Count()),
                            colLengths = colCells.Select(parr => parr.Count());

                        int[] candidates = {candidate};
                        if (rowLengths.Max() == amount && rowLengths.Min() > 0 &&
                            rowCells.Select(cells => cells.Select(c => c.Point.X)).UniteAll().Count() <= amount)
                        {
                            var row2D = rowCells.UniteAll();
                            if (puzzle.RemoveCandidates(
                                row2D.Select(c => puzzle.Columns[c.Point.X].Cells).UniteAll().Except(row2D),
                                candidates))
                            {
                                puzzle.LogAction(fishStr[amount], row2D, candidates);
                                return true;
                            }
                        }

                        if (colLengths.Max() == amount && colLengths.Min() > 0 &&
                            colCells.Select(cells => cells.Select(c => c.Point.Y)).UniteAll().Count() <= amount)
                        {
                            var col2D = colCells.UniteAll();
                            if (puzzle.RemoveCandidates(
                                col2D.Select(c => puzzle.Rows[c.Point.Y].Cells).UniteAll().Except(col2D), candidates))
                            {
                                puzzle.LogAction(fishStr[amount], col2D, candidates);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        for (var i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
                        {
                            indexes[loop] = i;
                            if (DoFish(loop + 1, indexes)) return true;
                        }
                    }

                    return false;
                }

                if (DoFish(0, new int[amount])) return true;
            }

            return false;
        }

        // Find hidden pairs/triples/quadruples
        private static bool FindHiddenTuples(Puzzle puzzle, Region region, int amount)
        {
            // If there are only "amount" cells with candidates, we don't have to waste our time
            if (region.Cells.Count(c => c.Candidates.Count > 0) == amount) return false;

            bool DoHiddenTuples(int loop, int[] candidates)
            {
                if (loop == amount)
                {
                    var cells = candidates.Select(c => region.GetCellsWithCandidates(c)).UniteAll();
                    var cands = cells.Select(c => c.Candidates).UniteAll();
                    if (cells.Count() != amount // There aren't "amount" cells for our tuple to be in
                        || cands.Count() ==
                        amount // We already know it's a tuple (might be faster to skip this check, idk)
                        || !cands.ContainsAll(candidates))
                        return false; // If a number in our combo doesn't actually show up in any of our cells
                    if (puzzle.RemoveCandidates(cells, Utils.OneToNine.Except(candidates)))
                    {
                        puzzle.LogAction("Hidden " + tupleStr[amount], cells, candidates);
                        return true;
                    }
                }
                else
                {
                    for (var i = candidates[loop == 0 ? loop : loop - 1] + 1; i <= 9; i++)
                    {
                        candidates[loop] = i;
                        if (DoHiddenTuples(loop + 1, candidates)) return true;
                    }
                }

                return false;
            }

            return DoHiddenTuples(0, new int[amount]);
        }

        // Find naked pairs/triples/quadruples
        private static bool FindNakedTuples(Puzzle puzzle, Region region, int amount)
        {
            bool DoNakedTuples(int loop, Cell[] cells, int[] indexes)
            {
                if (loop == amount)
                {
                    var combo = cells.Select(c => c.Candidates).UniteAll();
                    if (combo.Count() == amount)
                        if (puzzle.RemoveCandidates(
                            indexes.Select(i => region.Cells[i].GetCellsVisible()).IntersectAll(), combo))
                        {
                            puzzle.LogAction("Naked " + tupleStr[amount], cells, combo);
                            return true;
                        }
                }
                else
                {
                    for (var i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
                    {
                        var c = region.Cells[i];
                        if (c.Candidates.Count == 0) continue;
                        cells[loop] = c;
                        indexes[loop] = i;
                        if (DoNakedTuples(loop + 1, cells, indexes)) return true;
                    }
                }

                return false;
            }

            return DoNakedTuples(0, new Cell[amount], new int[amount]);
        }

        #endregion
    }
}