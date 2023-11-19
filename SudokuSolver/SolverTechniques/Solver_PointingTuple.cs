using System;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private static ReadOnlySpan<string> OrdinalStr => new string[4] { string.Empty, "1st", "2nd", "3rd" };

	private bool PointingTuple()
	{
		for (int i = 0; i < 3; i++)
		{
			var blockrow = new Cell[3][];
			var blockcol = new Cell[3][];
			for (int r = 0; r < 3; r++)
			{
				blockrow[r] = [.. Puzzle.Blocks[r + (i * 3)]];
				blockcol[r] = [.. Puzzle.Blocks[i + (r * 3)]];
			}

			for (int r = 0; r < 3; r++) // 3 blocks in a blockrow/blockcolumn
			{
				int[][] rowCandidates = new int[3][];
				int[][] colCand = new int[3][];
				for (int j = 0; j < 3; j++) // 3 rows/columns in block
				{
					// The 3 cells' candidates in a block's row/column
					rowCandidates[j] = blockrow[r].GetRowInBlock(j).Select(c => c.Candidates).UniteAll().ToArray();
					colCand[j] = blockcol[r].GetColumnInBlock(j).Select(c => c.Candidates).UniteAll().ToArray();
				}

				bool RemovePointingTuple(bool doRows, int rcIndex, IEnumerable<int> candidates)
				{
					bool changed = false;
					for (int j = 0; j < 3; j++)
					{
						if (j == r)
						{
							continue;
						}

						Cell[] rcs = doRows ? blockrow[j].GetRowInBlock(rcIndex) : blockcol[j].GetColumnInBlock(rcIndex);
						if (Cell.ChangeCandidates(rcs, candidates))
						{
							changed = true;
						}
					}

					if (changed)
					{
						ReadOnlySpan<Cell> culprits = doRows ? blockrow[r].GetRowInBlock(rcIndex) : blockcol[r].GetColumnInBlock(rcIndex);
						LogAction(TechniqueFormat("Pointing tuple",
							"Starting in block{0} {1}'s {2} block, {3} {0}: {4}",
							doRows ? "row" : "column", i + 1, OrdinalStr[r + 1], OrdinalStr[rcIndex + 1], candidates.SingleOrMultiToString()),
							culprits);
					}
					return changed;
				}

				// Now check if a row has a distinct candidate
				IEnumerable<int> zero_distinct = rowCandidates[0].Except(rowCandidates[1]).Except(rowCandidates[2]);
				if (zero_distinct.Any())
				{
					if (RemovePointingTuple(true, 0, zero_distinct))
					{
						return true;
					}
				}
				IEnumerable<int> one_distinct = rowCandidates[1].Except(rowCandidates[0]).Except(rowCandidates[2]);
				if (one_distinct.Any())
				{
					if (RemovePointingTuple(true, 1, one_distinct))
					{
						return true;
					}
				}
				IEnumerable<int> two_distinct = rowCandidates[2].Except(rowCandidates[0]).Except(rowCandidates[1]);
				if (two_distinct.Any())
				{
					if (RemovePointingTuple(true, 2, two_distinct))
					{
						return true;
					}
				}

				// Now check if a column has a distinct candidate
				zero_distinct = colCand[0].Except(colCand[1]).Except(colCand[2]);
				if (zero_distinct.Any())
				{
					if (RemovePointingTuple(false, 0, zero_distinct))
					{
						return true;
					}
				}
				one_distinct = colCand[1].Except(colCand[0]).Except(colCand[2]);
				if (one_distinct.Any())
				{
					if (RemovePointingTuple(false, 1, one_distinct))
					{
						return true;
					}
				}
				two_distinct = colCand[2].Except(colCand[0]).Except(colCand[1]);
				if (two_distinct.Any())
				{
					if (RemovePointingTuple(false, 2, two_distinct))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
