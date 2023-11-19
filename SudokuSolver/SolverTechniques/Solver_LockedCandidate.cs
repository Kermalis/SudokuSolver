using System;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool LockedCandidate()
	{
		for (int i = 0; i < 9; i++)
		{
			for (int candidate = 1; candidate <= 9; candidate++)
			{
				bool FindLockedCandidates(bool doRows)
				{
					IEnumerable<Cell> cellsWithCandidates = (doRows ? Puzzle.Rows : Puzzle.Columns)[i].GetCellsWithCandidate(candidate);

					// Even if a block only has these candidates for this "k" value, it'd be slower to check that before cancelling "BlacklistCandidates"
					if (cellsWithCandidates.Count() is 2 or 3)
					{
						int[] blocks = cellsWithCandidates.Select(c => c.Point.BlockIndex).Distinct().ToArray();
						if (blocks.Length == 1)
						{
							if (Cell.ChangeCandidates(Puzzle.Blocks[blocks[0]].Except(cellsWithCandidates), candidate))
							{
								LogAction(TechniqueFormat("Locked candidate",
									"{4} {0} locks within block {1}: {2}: {3}",
									doRows ? SPoint.RowLetter(i) : SPoint.ColumnLetter(i), blocks[0] + 1, cellsWithCandidates.Print(), candidate, doRows ? "Row" : "Column"), cellsWithCandidates);
								return true;
							}
						}
					}
					return false;
				}
				if (FindLockedCandidates(true) || FindLockedCandidates(false))
				{
					return true;
				}
			}
		}
		return false;
	}
}
