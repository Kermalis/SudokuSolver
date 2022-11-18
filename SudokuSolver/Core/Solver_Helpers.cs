using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver.Core;

internal sealed partial class Solver
{
	private static readonly string[] _fishStr = new string[5] { string.Empty, string.Empty, "X-Wing", "Swordfish", "Jellyfish" };
	private static readonly string[] _tupleStr = new string[5] { string.Empty, "single", "pair", "triple", "quadruple" };
	private static readonly string[] _ordinalStr = new string[4] { string.Empty, "1st", "2nd", "3rd" };

	// Find X-Wing, Swordfish & Jellyfish
	private static bool FindFish(Puzzle puzzle, int amount)
	{
		for (int candidate = 1; candidate <= 9; candidate++)
		{
			bool DoFish(int loop, int[] indexes)
			{
				if (loop == amount)
				{
					IEnumerable<IEnumerable<Cell>> rowCells = indexes.Select(i => puzzle.Rows[i].GetCellsWithCandidate(candidate)),
							colCells = indexes.Select(i => puzzle.Columns[i].GetCellsWithCandidate(candidate));

					IEnumerable<int> rowLengths = rowCells.Select(cells => cells.Count()),
							colLengths = colCells.Select(parr => parr.Count());

					if (rowLengths.Max() == amount && rowLengths.Min() > 0 && rowCells.Select(cells => cells.Select(c => c.Point.X)).UniteAll().Count() <= amount)
					{
						IEnumerable<Cell> row2D = rowCells.UniteAll();
						if (Cell.ChangeCandidates(row2D.Select(c => puzzle.Columns[c.Point.X]).UniteAll().Except(row2D), candidate))
						{
							puzzle.LogAction(Puzzle.TechniqueFormat(_fishStr[amount], "{0}: {1}", row2D.Print(), candidate), row2D, (Cell?)null);
							return true;
						}
					}
					if (colLengths.Max() == amount && colLengths.Min() > 0 && colCells.Select(cells => cells.Select(c => c.Point.Y)).UniteAll().Count() <= amount)
					{
						IEnumerable<Cell> col2D = colCells.UniteAll();
						if (Cell.ChangeCandidates(col2D.Select(c => puzzle.Rows[c.Point.Y]).UniteAll().Except(col2D), candidate))
						{
							puzzle.LogAction(Puzzle.TechniqueFormat(_fishStr[amount], "{0}: {1}", col2D.Print(), candidate), col2D, (Cell?)null);
							return true;
						}
					}
				}
				else
				{
					for (int i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
					{
						indexes[loop] = i;
						if (DoFish(loop + 1, indexes))
						{
							return true;
						}
					}
				}
				return false;
			}

			if (DoFish(0, new int[amount]))
			{
				return true;
			}
		}
		return false;
	}

	// Find hidden pairs/triples/quadruples
	private static bool FindHiddenTuples(Puzzle puzzle, Region region, int amount)
	{
		// If there are only "amount" cells with candidates, we don't have to waste our time
		if (region.Count(c => c.Candidates.Count > 0) == amount)
		{
			return false;
		}

		bool DoHiddenTuples(int loop, int[] candidates)
		{
			if (loop == amount)
			{
				IEnumerable<Cell> cells = candidates.Select(c => region.GetCellsWithCandidate(c)).UniteAll();
				IEnumerable<int> cands = cells.Select(c => c.Candidates).UniteAll();
				if (cells.Count() != amount // There aren't "amount" cells for our tuple to be in
					|| cands.Count() == amount // We already know it's a tuple (might be faster to skip this check, idk)
					|| !cands.ContainsAll(candidates))
				{
					return false; // If a number in our combo doesn't actually show up in any of our cells
				}
				if (Cell.ChangeCandidates(cells, Utils.OneToNine.Except(candidates)))
				{
					puzzle.LogAction(Puzzle.TechniqueFormat("Hidden " + _tupleStr[amount], "{0}: {1}", cells.Print(), candidates.Print()), cells, (Cell?)null);
					return true;
				}
			}
			else
			{
				for (int i = candidates[loop == 0 ? loop : loop - 1] + 1; i <= 9; i++)
				{
					candidates[loop] = i;
					if (DoHiddenTuples(loop + 1, candidates))
					{
						return true;
					}
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
				IEnumerable<int> combo = cells.Select(c => c.Candidates).UniteAll();
				if (combo.Count() == amount)
				{
					if (Cell.ChangeCandidates(indexes.Select(i => region[i].VisibleCells).IntersectAll(), combo))
					{
						puzzle.LogAction(Puzzle.TechniqueFormat("Naked " + _tupleStr[amount], "{0}: {1}", cells.Print(), combo.Print()), cells, (Cell?)null);
						return true;
					}
				}
			}
			else
			{
				for (int i = loop == 0 ? 0 : indexes[loop - 1] + 1; i < 9; i++)
				{
					Cell c = region[i];
					if (c.Candidates.Count == 0)
					{
						continue;
					}
					cells[loop] = c;
					indexes[loop] = i;
					if (DoNakedTuples(loop + 1, cells, indexes))
					{
						return true;
					}
				}
			}
			return false;
		}

		return DoNakedTuples(0, new Cell[amount], new int[amount]);
	}
}
