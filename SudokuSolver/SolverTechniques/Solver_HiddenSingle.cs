using System.Collections.ObjectModel;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool HiddenSingle()
	{
		bool changed = false;

		for (int i = 0; i < 9; i++)
		{
			foreach (ReadOnlyCollection<Region> region in Puzzle.Regions)
			{
				for (int candidate = 1; candidate <= 9; candidate++)
				{
					Cell[] c = region[i].GetCellsWithCandidate(candidate).ToArray();
					if (c.Length == 1)
					{
						c[0].Set(candidate);
						LogAction(TechniqueFormat("Hidden single", "{0}: {1}", c[0], candidate), c[0]);
						changed = true;
					}
				}
			}
		}

		return changed;
	}
}
