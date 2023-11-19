using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool XYChain()
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				if (cell.Candidates.Count != 2)
				{
					continue; // Must have two candidates
				}

				var ignore = new List<Cell>();
				int start1 = cell.Candidates.ElementAt(0);
				int start2 = cell.Candidates.ElementAt(1);
				if (XYChain_Recursion(cell, ignore, cell, start1, start2)
					|| XYChain_Recursion(cell, ignore, cell, start2, start1))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool XYChain_Recursion(Cell startCell, List<Cell> ignore, Cell currentCell, int theOneThatWillEndItAllBaybee, int mustFind)
	{
		ignore.Add(currentCell);
		IEnumerable<Cell> visible = currentCell.VisibleCells.Except(ignore);
		foreach (Cell cell in visible)
		{
			if (!cell.Candidates.TryGetCount2(out int can1, out int can2))
			{
				continue; // Must have two candidates
			}
			if (can1 != mustFind && can2 != mustFind)
			{
				continue; // Must have "mustFind"
			}

			int otherCandidate = mustFind == can1 ? can2 : can1;
			// Check end condition
			if (otherCandidate == theOneThatWillEndItAllBaybee && startCell != currentCell)
			{
				Cell[] commonVisibleWithStartCell = cell.VisibleCells.Intersect(startCell.VisibleCells).ToArray();
				if (commonVisibleWithStartCell.Length > 0)
				{
					IEnumerable<Cell> commonWithEndingCandidate = commonVisibleWithStartCell.Where(c => c.Candidates.Contains(theOneThatWillEndItAllBaybee));
					if (Cell.ChangeCandidates(commonWithEndingCandidate, theOneThatWillEndItAllBaybee))
					{
						ignore.Remove(startCell); // Remove here because we're now using "ignore" as "semiCulprits" and exiting
						Cell[] culprits = [startCell, cell];
						LogAction(TechniqueFormat("XY-Chain",
							"{0}-{1}: {2}",
							Utils.PrintCells(culprits), ignore.SingleOrMultiToString(), theOneThatWillEndItAllBaybee),
							culprits, ignore);
						return true;
					}
				}
			}
			// Loop again
			if (XYChain_Recursion(startCell, ignore, cell, theOneThatWillEndItAllBaybee, otherCandidate))
			{
				return true;
			}
		}
		ignore.Remove(currentCell);
		return false;
	}
}
