using System;
using System.Diagnostics.CodeAnalysis;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	// TODO: Comments
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
		Span<Cell> cellsWith2Cand = _cellCache.AsSpan(0, 9);
		cellsWith2Cand = region.GetCellsWithCandidateCount(2, cellsWith2Cand);
		if (cellsWith2Cand.Length < 2)
		{
			return false;
		}

		for (int i = 0; i < cellsWith2Cand.Length; i++)
		{
			Cell c1 = cellsWith2Cand[i];

			for (int j = i + 1; j < cellsWith2Cand.Length; j++)
			{
				Cell c2 = cellsWith2Cand[j];

				if (!YWing_GetSingleSharedCandidate(c1.CandI, c2.CandI, out int other1, out int other2))
				{
					continue;
				}

				if (YWing_Match(cellsWith2Cand, other1, other2, c1, c2))
				{
					return true;
				}
				if (YWing_Match(cellsWith2Cand, other1, other2, c2, c1))
				{
					return true;
				}
			}
		}

		return false;
	}
	private static bool YWing_GetSingleSharedCandidate(Candidates a, Candidates b, out int other1, out int other2)
	{
		// These two cells only have 2 candidates each.
		// For example:
		// 4 and 5
		// 3 and 5
		// ...1 shared candidate.

		a.GetCount2(out int a1, out int a2);
		b.GetCount2(out int b1, out int b2);

		int sharedCount = 0;
		int sharedCandidate = -1;

		if (a1 == b1)
		{
			sharedCandidate = a1;
			sharedCount++;
		}
		if (a1 == b2)
		{
			sharedCandidate = a1;
			sharedCount++;
		}
		if (a2 == b1)
		{
			sharedCandidate = a2;
			sharedCount++;
		}
		if (a2 == b2)
		{
			sharedCandidate = a2;
			sharedCount++;
		}

		if (sharedCount != 1)
		{
			other1 = -1;
			other2 = -1;
			return false;
		}

		other1 = sharedCandidate == a1 ? a2 : a1;
		other2 = sharedCandidate == b1 ? b2 : b1;
		return true;
	}
	private bool YWing_Match(ReadOnlySpan<Cell> cellsWith2Cand, int other1, int other2, Cell cell, Cell cOther)
	{
		// Example: c1 and c3 see each other, so remove similarities from c2 and c3
		if (!YWing_FindSingleMatch(cell, cellsWith2Cand, other1, other2, out Cell? c3))
		{
			return false;
		}

		Span<Cell> commonCells = _cellCache.AsSpan(9, 7);
		commonCells = c3.IntersectVisibleCells(cOther, commonCells);

		Span<int> commonCandidates = stackalloc int[1];
		commonCandidates = c3.CandI.Intersect(cOther.CandI, commonCandidates); // Will just be 1 candidate
		int candidate = commonCandidates[0];
		if (Candidates.Set(commonCells, candidate, false))
		{
			Span<Cell> culprits = _cellCache.AsSpan(0, 3);
			culprits[0] = cell;
			culprits[1] = cOther;
			culprits[2] = c3;

			LogAction(TechniqueFormat("Y-Wing",
				"{0}: {1}",
				Utils.PrintCells(culprits), candidate),
				culprits);
			return true;
		}
		return false;
	}
	private static bool YWing_FindSingleMatch(Cell cell, ReadOnlySpan<Cell> cellsWith2Cand, int other1, int other2, [NotNullWhen(true)] out Cell? c3)
	{
		// Desperately need comments!
		c3 = null;

		foreach (Cell c in cell.VisibleI)
		{
			if (cellsWith2Cand.SimpleIndexOf(c) != -1)
			{
				continue;
			}

			if (c.CandI.Count == 2 && c.CandI.HasBoth(other1, other2))
			{
				if (c3 is not null)
				{
					return false; // More than 1 match
				}
				c3 = c; // Our current match
			}
		}

		return c3 is not null;
	}
}