using System;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private static ReadOnlySpan<string> FishStr => new string[5] { string.Empty, string.Empty, "X-Wing", "Swordfish", "Jellyfish" };

	private bool Jellyfish()
	{
		return Fish_Find(4);
	}

	private bool Swordfish()
	{
		return Fish_Find(3);
	}

	private bool XWing()
	{
		return Fish_Find(2);
	}


	private bool Fish_Find(int amount)
	{
		for (int candidate = 1; candidate <= 9; candidate++)
		{
			if (Fish_Recurse(amount, candidate, 0, new int[amount]))
			{
				return true;
			}
		}
		return false;
	}
	private bool Fish_Recurse(int amount, int candidate, int loop, int[] indexes)
	{
		if (loop == amount)
		{
			IEnumerable<IEnumerable<Cell>> rowCells = indexes.Select(i => Puzzle.RowsI[i].GetCellsWithCandidate(candidate));
			IEnumerable<IEnumerable<Cell>> colCells = indexes.Select(i => Puzzle.ColumnsI[i].GetCellsWithCandidate(candidate));

			IEnumerable<int> rowLengths = rowCells.Select(cells => cells.Count());
			IEnumerable<int> colLengths = colCells.Select(parr => parr.Count());

			if (rowLengths.Max() == amount && rowLengths.Min() > 0 && rowCells.Select(cells => cells.Select(c => c.Point.Column)).UniteAll().Count() <= amount)
			{
				IEnumerable<Cell> row2D = rowCells.UniteAll();
				if (Cell.ChangeCandidates(row2D.Select(c => Puzzle.ColumnsI[c.Point.Column]).UniteAll().Except(row2D), candidate))
				{
					LogAction(TechniqueFormat(FishStr[amount], "{0}: {1}", row2D.Print(), candidate), row2D);
					return true;
				}
			}
			if (colLengths.Max() == amount && colLengths.Min() > 0 && colCells.Select(cells => cells.Select(c => c.Point.Row)).UniteAll().Count() <= amount)
			{
				IEnumerable<Cell> col2D = colCells.UniteAll();
				if (Cell.ChangeCandidates(col2D.Select(c => Puzzle.RowsI[c.Point.Row]).UniteAll().Except(col2D), candidate))
				{
					LogAction(TechniqueFormat(FishStr[amount], "{0}: {1}", col2D.Print(), candidate), col2D);
					return true;
				}
			}
		}
		else
		{
			for (int i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
			{
				indexes[loop] = i;
				if (Fish_Recurse(amount, candidate, loop + 1, indexes))
				{
					return true;
				}
			}
		}
		return false;
	}
}
