using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool XYChain()
	{
		bool Recursion(Cell startCell, List<Cell> ignore, Cell currentCell, int theOneThatWillEndItAllBaybee, int mustFind)
		{
			ignore.Add(currentCell);
			IEnumerable<Cell> visible = currentCell.VisibleCells.Except(ignore);
			foreach (Cell cell in visible)
			{
				if (cell.Candidates.Count != 2)
				{
					continue; // Must have two candidates
				}
				if (!cell.Candidates.Contains(mustFind))
				{
					continue; // Must have "mustFind"
				}

				int otherCandidate = cell.Candidates.Except(new int[] { mustFind }).Single();
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
							var culprits = new Cell[] { startCell, cell };
							LogAction(TechniqueFormat("XY-Chain", "{0}-{1}: {2}", culprits.Print(), ignore.SingleOrMultiToString(), theOneThatWillEndItAllBaybee), culprits, ignore);
							return true;
						}
					}
				}
				// Loop again
				if (Recursion(startCell, ignore, cell, theOneThatWillEndItAllBaybee, otherCandidate))
				{
					return true;
				}
			}
			ignore.Remove(currentCell);
			return false;
		}

		for (int x = 0; x < 9; x++)
		{
			for (int y = 0; y < 9; y++)
			{
				Cell cell = Puzzle[x, y];
				if (cell.Candidates.Count != 2)
				{
					continue; // Must have two candidates
				}
				var ignore = new List<Cell>();
				int start1 = cell.Candidates.ElementAt(0);
				int start2 = cell.Candidates.ElementAt(1);
				if (Recursion(cell, ignore, cell, start1, start2) || Recursion(cell, ignore, cell, start2, start1))
				{
					return true;
				}
			}
		}
		return false;
	}
}
