using System;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool HiddenSingle()
	{
		bool changed = false;
		bool restartSearch;

		do
		{
			restartSearch = false;

			foreach (Region[] regions in Puzzle.RegionsI)
			{
				foreach (Region region in regions)
				{
					for (int digit = 1; digit <= 9; digit++)
					{
						Span<Cell> c = region.GetCellsWithCandidate(digit, _cellCache);
						if (c.Length == 1)
						{
							c[0].SetValue(digit);
							LogAction(TechniqueFormat("Hidden single",
								"{0}: {1}",
								c[0], digit),
								c[0]);
							changed = true;
							restartSearch = true;
						}
					}
				}
			}

		} while (restartSearch);

		return changed;
	}
}