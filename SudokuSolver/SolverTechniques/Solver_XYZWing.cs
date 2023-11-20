using System;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool XYZWing()
	{
		for (int i = 0; i < 9; i++)
		{
			if (XYZWing_Find(Puzzle.Rows[i]) || XYZWing_Find(Puzzle.Columns[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool XYZWing_Find(Region region)
	{
		Cell[] cells2 = region.Where(c => c.CandI.Count == 2).ToArray();
		if (cells2.Length == 0)
		{
			return false;
		}

		Cell[] cells3 = region.Where(c => c.CandI.Count == 3).ToArray();
		if (cells3.Length == 0)
		{
			return false;
		}

		bool changed = false;
		foreach (Cell c2 in cells2)
		{
			foreach (Cell c3 in cells3)
			{
				if (c2.CandI.Intersect(c3.CandI).Count() != 2)
				{
					continue;
				}

				IEnumerable<Cell> c3Sees = c3.VisibleCells.Except(region)
								.Where(c => c.CandI.Count == 2 // If it has 2 candidates
								&& c.CandI.Intersect(c3.CandI).Count() == 2 // Shares them both with c3
								&& c.CandI.Intersect(c2.CandI).Count() == 1); // And shares one with c2
				foreach (Cell c2_2 in c3Sees)
				{
					IEnumerable<Cell> allSee = c2.VisibleCells.Intersect(c3.VisibleCells).Intersect(c2_2.VisibleCells);
					int allHave = c2.CandI.Intersect(c3.CandI).Intersect(c2_2.CandI).Single(); // Will be 1 Length

					if (Cell.ChangeCandidates(allSee, allHave))
					{
						Span<Cell> culprits = _cellCache.AsSpan(0, 3);
						culprits[0] = c2;
						culprits[1] = c3;
						culprits[2] = c2_2;

						LogAction(TechniqueFormat("XYZ-Wing",
							"{0}: {1}",
							Utils.PrintCells(culprits), allHave),
							culprits);
						changed = true;
					}
				}
			}
		}
		return changed;
	}
}
