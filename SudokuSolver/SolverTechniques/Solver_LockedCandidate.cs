using System;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool LockedCandidate()
	{
		// A locked candidate is when a row or column has a candidate that can only appear in one block.
		// For example, if a row has 3 cells with "5" as a candidate, and all 3 of those cells are in the same block, then that block can only have "5" in that row.
		// Other cells in that block cannot have "5", so we clear candidates that way.

		// Our search will iterate each row and column and check for these conditions on each digit.

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
		// Grab the row or column we will scan.
		Region region = (doRows ? Puzzle.RowsI : Puzzle.ColumnsI)[i];

		// Grab the cells in this row/col that still have the candidate we're looking for.
		Span<Cell> cellsWithCandidates = _cellCache.AsSpan(0, 9);
		cellsWithCandidates = region.GetCellsWithCandidate(candidate, cellsWithCandidates);

		// If there are 2 or 3 cells with the candidate, we will check to see if they share a block.
		if (cellsWithCandidates.Length is not 2 and not 3)
		{
			return false;
		}

		// Check which blocks the cells belong to.
		Span<int> blockIndices = stackalloc int[3];
		blockIndices = Utils.GetDistinctBlockIndices(cellsWithCandidates, blockIndices);

		// If they are in the same block, we can remove the candidate from other cells in the block.
		if (blockIndices.Length != 1)
		{
			return false;
		}

		// Grab the block they're in.
		int blockIndex = blockIndices[0];
		Region block = Puzzle.BlocksI[blockIndex];

		// Grab the cells in the block that don't belong to the col/row we scanned. They cannot have this candidate.
		// Up to 6 cells can have candidates changed.
		Span<Cell> cellsToChange = _cellCache.AsSpan(3, 6);
		cellsToChange = block.Except(region, cellsToChange);

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