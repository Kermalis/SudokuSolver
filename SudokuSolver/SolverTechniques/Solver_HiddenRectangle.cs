using System;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

partial class Solver
{
	private bool HiddenRectangle()
	{
		for (int x1 = 0; x1 < 9; x1++)
		{
			Region c1 = Puzzle.ColumnsI[x1];
			for (int x2 = x1 + 1; x2 < 9; x2++)
			{
				Region c2 = Puzzle.ColumnsI[x2];
				for (int y1 = 0; y1 < 9; y1++)
				{
					for (int y2 = y1 + 1; y2 < 9; y2++)
					{
						for (int value1 = 1; value1 <= 9; value1++)
						{
							for (int value2 = value1 + 1; value2 <= 9; value2++)
							{
								int[] candidates = [value1, value2];
								Cell[] cells = [c1[y1], c1[y2], c2[y1], c2[y2]];
								if (cells.Any(c => !c.CandI.ContainsAll(candidates)))
								{
									continue;
								}
								ILookup<int, Cell> l = cells.ToLookup(c => c.CandI.Count);
								IEnumerable<Cell> gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g);
								int gtTwoCount = gtTwo.Count();
								if (gtTwoCount < 2 || gtTwoCount > 3)
								{
									continue;
								}

								bool changed = false;
								foreach (Cell c in l[2])
								{
									int eks = c.Point.Column == x1 ? x2 : x1;
									int why = c.Point.Row == y1 ? y2 : y1;
									foreach (int i in candidates)
									{
										if (!Puzzle.RowsI[why].GetCellsWithCandidate(i).Except(cells).Any() // "i" only appears in our UR
											&& !Puzzle.ColumnsI[eks].GetCellsWithCandidate(i).Except(cells).Any())
										{
											Cell diag = Puzzle[eks, why];
											if (diag.CandI.Count == 2)
											{
												diag.SetValue(i);
											}
											else
											{
												diag.CandI.Set(i == value1 ? value2 : value1, false);
											}
											changed = true;
										}
									}
								}
								if (changed)
								{
									LogAction(TechniqueFormat("Hidden rectangle",
										"{0}: {1}",
										Utils.PrintCells(cells), Utils.PrintCandidates(candidates)),
										(ReadOnlySpan<Cell>)cells);
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}
}
