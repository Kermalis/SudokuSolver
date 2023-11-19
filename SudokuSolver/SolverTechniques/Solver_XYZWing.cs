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
		bool changed = false;
		Cell[] cells2 = region.Where(c => c.Candidates.Count == 2).ToArray();
		Cell[] cells3 = region.Where(c => c.Candidates.Count == 3).ToArray();
		if (cells2.Length > 0 && cells3.Length > 0)
		{
			for (int j = 0; j < cells2.Length; j++)
			{
				Cell c2 = cells2[j];
				for (int k = 0; k < cells3.Length; k++)
				{
					Cell c3 = cells3[k];
					if (c2.Candidates.Intersect(c3.Candidates).Count() != 2)
					{
						continue;
					}

					IEnumerable<Cell> c3Sees = c3.VisibleCells.Except(region)
								.Where(c => c.Candidates.Count == 2 // If it has 2 candidates
								&& c.Candidates.Intersect(c3.Candidates).Count() == 2 // Shares them both with p3
								&& c.Candidates.Intersect(c2.Candidates).Count() == 1); // And shares one with p2
					foreach (Cell c2_2 in c3Sees)
					{
						IEnumerable<Cell> allSee = c2.VisibleCells.Intersect(c3.VisibleCells).Intersect(c2_2.VisibleCells);
						int allHave = c2.Candidates.Intersect(c3.Candidates).Intersect(c2_2.Candidates).Single(); // Will be 1 Length
						if (Cell.ChangeCandidates(allSee, allHave))
						{
							var culprits = new Cell[] { c2, c3, c2_2 };
							LogAction(TechniqueFormat("XYZ-Wing", "{0}: {1}", culprits.Print(), allHave), culprits);
							changed = true;
						}
					}
				}
			}
		}
		return changed;
	}
}
