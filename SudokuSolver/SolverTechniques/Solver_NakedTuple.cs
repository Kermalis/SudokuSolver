using System;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool NakedQuadruple()
	{
		for (int i = 0; i < 9; i++)
		{
			if (NakedTuple_Find(Puzzle.Blocks[i], 4)
				|| NakedTuple_Find(Puzzle.Rows[i], 4)
				|| NakedTuple_Find(Puzzle.Columns[i], 4))
			{
				return true;
			}
		}
		return false;
	}

	private bool NakedTriple()
	{
		for (int i = 0; i < 9; i++)
		{
			if (NakedTuple_Find(Puzzle.Blocks[i], 3)
				|| NakedTuple_Find(Puzzle.Rows[i], 3)
				|| NakedTuple_Find(Puzzle.Columns[i], 3))
			{
				return true;
			}
		}
		return false;
	}

	private bool NakedPair()
	{
		for (int i = 0; i < 9; i++)
		{
			if (NakedTuple_Find(Puzzle.Blocks[i], 2)
				|| NakedTuple_Find(Puzzle.Rows[i], 2)
				|| NakedTuple_Find(Puzzle.Columns[i], 2))
			{
				return true;
			}
		}
		return false;
	}

	private bool NakedTuple_Find(Region region, int amount)
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
						LogAction(TechniqueFormat("Naked " + TupleStr[amount], "{0}: {1}", cells.Print(), combo.Print()), cells);
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
