using System;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool YWing()
	{
		for (int i = 0; i < 9; i++)
		{
			if (YWing_Find(Puzzle.Rows[i]) || YWing_Find(Puzzle.Columns[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool YWing_Find(Region region)
	{
		Cell[] cells = region.Where(c => c.CandI.Count == 2).ToArray();
		if (cells.Length < 2)
		{
			return false;
		}

		for (int j = 0; j < cells.Length; j++)
		{
			Cell c1 = cells[j];
			for (int k = j + 1; k < cells.Length; k++)
			{
				Cell c2 = cells[k];
				IEnumerable<int> inter = c1.CandI.Intersect(c2.CandI);
				if (inter.Count() != 1)
				{
					continue;
				}

				int other1 = c1.CandI.Except(inter).ElementAt(0);
				int other2 = c2.CandI.Except(inter).ElementAt(0);

				var a = new Cell[] { c1, c2 };
				foreach (Cell cell in a)
				{
					IEnumerable<Cell> c3a = cell.VisibleCells.Except(cells).Where(c => c.CandI.Count == 2 && c.CandI.Intersect(new int[] { other1, other2 }).Count() == 2);
					if (c3a.Count() == 1) // Example: p1 and p3 see each other, so remove similarities from p2 and p3
					{
						Cell c3 = c3a.ElementAt(0);
						Cell cOther = a.Single(c => c != cell);
						IEnumerable<Cell> commonCells = cOther.VisibleCells.Intersect(c3.VisibleCells);
						int candidate = cOther.CandI.Intersect(c3.CandI).Single(); // Will just be 1 candidate
						if (Cell.ChangeCandidates(commonCells, candidate))
						{
							ReadOnlySpan<Cell> culprits = [c1, c2, c3];
							LogAction(TechniqueFormat("Y-Wing",
								"{0}: {1}",
								Utils.PrintCells(culprits), candidate),
								culprits);
							return true;
						}
					}
				}
			}
		}
		return false;
	}
}
