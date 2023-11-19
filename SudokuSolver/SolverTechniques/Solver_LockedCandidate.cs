using System;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool LockedCandidate()
	{
		for (int i = 0; i < 9; i++)
		{
			for (int candidate = 1; candidate <= 9; candidate++)
			{
				if (LockedCandidate_Find(i, candidate, true)
					|| LockedCandidate_Find(i, candidate, false))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool LockedCandidate_Find(int i, int candidate, bool doRows)
	{
		Region region = (doRows ? Puzzle.Rows : Puzzle.Columns)[i];

		Span<Cell> cellsWithCandidates = _cellCache.AsSpan(0, 9);
		cellsWithCandidates = region.GetCellsWithCandidate(candidate, cellsWithCandidates);

		// Even if a block only has these candidates for this "i" value, it'd be slower to check that before cancelling "BlacklistCandidates"
		if (cellsWithCandidates.Length is not 2 and not 3)
		{
			return false;
		}

		Span<int> blockIndices = _intCache.AsSpan(0, cellsWithCandidates.Length);
		blockIndices = Utils.GetDistinctBlockIndices(cellsWithCandidates, blockIndices);

		if (blockIndices.Length != 1)
		{
			return false;
		}

		int blockIndex = blockIndices[0];
		Region block = Puzzle.Blocks[blockIndex];

		Span<Cell> cellsToChange = _cellCache.AsSpan(9, 9);
		cellsToChange = block.Except(cellsWithCandidates, cellsToChange);

		if (Candidates.Set(cellsToChange, candidate, false))
		{
			LogAction(TechniqueFormat("Locked candidate",
				"{4} {0} locks within block {1}: {2}: {3}",
				doRows ? SPoint.RowLetter(i) : SPoint.ColumnLetter(i), blockIndex + 1, Utils.PrintCells(cellsWithCandidates), candidate, doRows ? "Row" : "Column"),
				cellsWithCandidates);
			return true;
		}
		return false;
	}
}
