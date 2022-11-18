using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kermalis.SudokuSolver.Core;

internal sealed partial class Solver
{
	private static bool AvoidableRectangle(Puzzle puzzle)
	{
		for (int type = 1; type <= 2; type++)
		{
			for (int x1 = 0; x1 < 9; x1++)
			{
				Region c1 = puzzle.Columns[x1];
				for (int x2 = x1 + 1; x2 < 9; x2++)
				{
					Region c2 = puzzle.Columns[x2];
					for (int y1 = 0; y1 < 9; y1++)
					{
						for (int y2 = y1 + 1; y2 < 9; y2++)
						{
							for (int value1 = 1; value1 <= 9; value1++)
							{
								for (int value2 = value1 + 1; value2 <= 9; value2++)
								{
									int[] candidates = new int[] { value1, value2 };
									var cells = new Cell[] { c1[y1], c1[y2], c2[y1], c2[y2] };
									if (cells.Any(c => c.OriginalValue != Cell.EMPTY_VALUE))
									{
										continue;
									}

									IEnumerable<Cell> alreadySet = cells.Where(c => c.Value != Cell.EMPTY_VALUE),
											notSet = cells.Where(c => c.Value == Cell.EMPTY_VALUE);

									switch (type)
									{
										case 1:
										{
											if (alreadySet.Count() != 3)
											{
												continue;
											}
											break;
										}
										case 2:
										{
											if (alreadySet.Count() != 2)
											{
												continue;
											}
											break;
										}
									}
									var pairs = new Cell[][]
										{
											new Cell[] { cells[0], cells[3] },
											new Cell[] { cells[1], cells[2] }
										};
									foreach (Cell[] pair in pairs)
									{
										Cell[] otherPair = pair == pairs[0] ? pairs[1] : pairs[0];
										foreach (int i in candidates)
										{
											int otherVal = candidates.Single(ca => ca != i);
											if (((pair[0].Value == i && pair[1].Value == Cell.EMPTY_VALUE && pair[1].Candidates.Count == 2 && pair[1].Candidates.Contains(i))
												|| (pair[1].Value == i && pair[0].Value == Cell.EMPTY_VALUE && pair[0].Candidates.Count == 2 && pair[0].Candidates.Contains(i)))
												&& otherPair.All(c => c.Value == otherVal || (c.Candidates.Count == 2 && c.Candidates.Contains(otherVal))))
											{
												goto breakpairs;
											}
										}
									}
									continue; // Did not find
								breakpairs:
									bool changed = false;
									switch (type)
									{
										case 1:
										{
											Cell cell = notSet.ElementAt(0);
											if (cell.Candidates.Count == 2)
											{
												cell.Set(cell.Candidates.Except(candidates).ElementAt(0));
											}
											else
											{
												cell.ChangeCandidates(cell.Candidates.Intersect(candidates));
											}
											changed = true;
											break;
										}
										case 2:
										{
											IEnumerable<int> commonCandidates = notSet.Select(c => c.Candidates.Except(candidates)).IntersectAll();
											if (commonCandidates.Any()
												&& Cell.ChangeCandidates(notSet.Select(c => c.VisibleCells).IntersectAll(), commonCandidates))
											{
												changed = true;
											}
											break;
										}
									}

									if (changed)
									{
										puzzle.LogAction(Puzzle.TechniqueFormat("Avoidable rectangle", "{0}: {1}", cells.Print(), candidates.Print()), cells, (Cell?)null);
										return true;
									}
								}
							}
						}
					}
				}
			}
		}
		return false;
	}

	private static bool HiddenRectangle(Puzzle puzzle)
	{
		for (int x1 = 0; x1 < 9; x1++)
		{
			Region c1 = puzzle.Columns[x1];
			for (int x2 = x1 + 1; x2 < 9; x2++)
			{
				Region c2 = puzzle.Columns[x2];
				for (int y1 = 0; y1 < 9; y1++)
				{
					for (int y2 = y1 + 1; y2 < 9; y2++)
					{
						for (int value1 = 1; value1 <= 9; value1++)
						{
							for (int value2 = value1 + 1; value2 <= 9; value2++)
							{
								int[] candidates = new int[] { value1, value2 };
								var cells = new Cell[] { c1[y1], c1[y2], c2[y1], c2[y2] };
								if (cells.Any(c => !c.Candidates.ContainsAll(candidates)))
								{
									continue;
								}
								ILookup<int, Cell> l = cells.ToLookup(c => c.Candidates.Count);
								IEnumerable<Cell> gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g);
								int gtTwoCount = gtTwo.Count();
								if (gtTwoCount < 2 || gtTwoCount > 3)
								{
									continue;
								}

								bool changed = false;
								foreach (Cell c in l[2])
								{
									int eks = c.Point.X == x1 ? x2 : x1,
											why = c.Point.Y == y1 ? y2 : y1;
									foreach (int i in candidates)
									{
										if (!puzzle.Rows[why].GetCellsWithCandidate(i).Except(cells).Any() // "i" only appears in our UR
											&& !puzzle.Columns[eks].GetCellsWithCandidate(i).Except(cells).Any())
										{
											Cell diag = puzzle[eks, why];
											if (diag.Candidates.Count == 2)
											{
												diag.Set(i);
											}
											else
											{
												diag.ChangeCandidates(i == value1 ? value2 : value1);
											}
											changed = true;
										}
									}
								}
								if (changed)
								{
									puzzle.LogAction(Puzzle.TechniqueFormat("Hidden rectangle", "{0}: {1}", cells.Print(), candidates.Print()), cells, (Cell?)null);
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

	private static bool UniqueRectangle(Puzzle puzzle)
	{
		for (int type = 1; type <= 6; type++) // Type
		{
			for (int x1 = 0; x1 < 9; x1++)
			{
				Region c1 = puzzle.Columns[x1];
				for (int x2 = x1 + 1; x2 < 9; x2++)
				{
					Region c2 = puzzle.Columns[x2];
					for (int y1 = 0; y1 < 9; y1++)
					{
						for (int y2 = y1 + 1; y2 < 9; y2++)
						{
							for (int value1 = 1; value1 <= 9; value1++)
							{
								for (int value2 = value1 + 1; value2 <= 9; value2++)
								{
									int[] candidates = new int[] { value1, value2 };
									var cells = new Cell[] { c1[y1], c1[y2], c2[y1], c2[y2] };
									if (cells.Any(c => !c.Candidates.ContainsAll(candidates)))
									{
										continue;
									}

									ILookup<int, Cell> l = cells.ToLookup(c => c.Candidates.Count);
									Cell[] gtTwo = l.Where(g => g.Key > 2).SelectMany(g => g).ToArray(),
											two = l[2].ToArray(), three = l[3].ToArray(), four = l[4].ToArray();

									switch (type) // Check for candidate counts
									{
										case 1:
										{
											if (two.Length != 3 || gtTwo.Length != 1)
											{
												continue;
											}
											break;
										}
										case 2:
										case 6:
										{
											if (two.Length != 2 || three.Length != 2)
											{
												continue;
											}
											break;
										}
										case 3:
										{
											if (two.Length != 2 || gtTwo.Length != 2)
											{
												continue;
											}
											break;
										}
										case 4:
										{
											if (two.Length != 2 || three.Length != 1 || four.Length != 1)
											{
												continue;
											}
											break;
										}
										case 5:
										{
											if (two.Length != 1 || three.Length != 3)
											{
												continue;
											}
											break;
										}
									}

									switch (type) // Check for extra rules
									{
										case 1:
										{
											if (gtTwo[0].Candidates.Count == 3)
											{
												gtTwo[0].Set(gtTwo[0].Candidates.Single(c => !candidates.Contains(c)));
											}
											else
											{
												Cell.ChangeCandidates(gtTwo, candidates);
											}
											break;
										}
										case 2:
										{
											if (!three[0].Candidates.SetEquals(three[1].Candidates))
											{
												continue;
											}
											if (!Cell.ChangeCandidates(three[0].VisibleCells.Intersect(three[1].VisibleCells), three[0].Candidates.Except(candidates)))
											{
												continue;
											}
											break;
										}
										case 3:
										{
											if (gtTwo[0].Point.X != gtTwo[1].Point.X && gtTwo[0].Point.Y != gtTwo[1].Point.Y)
											{
												continue; // Must be non-diagonal
											}
											IEnumerable<int> others = gtTwo[0].Candidates.Except(candidates).Union(gtTwo[1].Candidates.Except(candidates));
											if (others.Count() > 4 || others.Count() < 2)
											{
												continue;
											}
											IEnumerable<Cell> nSubset = ((gtTwo[0].Point.Y == gtTwo[1].Point.Y) ? // Same row
														puzzle.Rows[gtTwo[0].Point.Y] : puzzle.Columns[gtTwo[0].Point.X])
														.Where(c => c.Candidates.ContainsAny(others) && !c.Candidates.ContainsAny(Utils.OneToNine.Except(others)));
											if (nSubset.Count() != others.Count() - 1)
											{
												continue;
											}
											if (!Cell.ChangeCandidates(nSubset.Union(gtTwo).Select(c => c.VisibleCells).IntersectAll(), others))
											{
												continue;
											}
											break;
										}
										case 4:
										{
											int[] remove = new int[1];
											if (four[0].Point.BlockIndex == three[0].Point.BlockIndex)
											{
												if (puzzle.Blocks[four[0].Point.BlockIndex].GetCellsWithCandidate(value1).Count() == 2)
												{
													remove[0] = value2;
												}
												else if (puzzle.Blocks[four[0].Point.BlockIndex].GetCellsWithCandidate(value2).Count() == 2)
												{
													remove[0] = value1;
												}
											}
											if (remove[0] != 0) // They share the same row/column but not the same block
											{
												if (three[0].Point.X == three[0].Point.X)
												{
													if (puzzle.Columns[four[0].Point.X].GetCellsWithCandidate(value1).Count() == 2)
													{
														remove[0] = value2;
													}
													else if (puzzle.Columns[four[0].Point.X].GetCellsWithCandidate(value2).Count() == 2)
													{
														remove[0] = value1;
													}
												}
												else
												{
													if (puzzle.Rows[four[0].Point.Y].GetCellsWithCandidate(value1).Count() == 2)
													{
														remove[0] = value2;
													}
													else if (puzzle.Rows[four[0].Point.Y].GetCellsWithCandidate(value2).Count() == 2)
													{
														remove[0] = value1;
													}
												}
											}
											else
											{
												continue;
											}
											Cell.ChangeCandidates(cells.Except(l[2]), remove);
											break;
										}
										case 5:
										{
											if (!three[0].Candidates.SetEquals(three[1].Candidates) || !three[1].Candidates.SetEquals(three[2].Candidates))
											{
												continue;
											}
											if (!Cell.ChangeCandidates(three.Select(c => c.VisibleCells).IntersectAll(), three[0].Candidates.Except(candidates)))
											{
												continue;
											}
											break;
										}
										case 6:
										{
											if (three[0].Point.X == three[1].Point.X)
											{
												continue;
											}
											int set;
											if (c1.GetCellsWithCandidate(value1).Count() == 2 && c2.GetCellsWithCandidate(value1).Count() == 2 // Check if "v" only appears in the UR
												&& puzzle.Rows[two[0].Point.Y].GetCellsWithCandidate(value1).Count() == 2
													&& puzzle.Rows[two[1].Point.Y].GetCellsWithCandidate(value1).Count() == 2)
											{
												set = value1;
											}
											else if (c1.GetCellsWithCandidate(value2).Count() == 2 && c2.GetCellsWithCandidate(value2).Count() == 2
												&& puzzle.Rows[two[0].Point.Y].GetCellsWithCandidate(value2).Count() == 2
													&& puzzle.Rows[two[1].Point.Y].GetCellsWithCandidate(value2).Count() == 2)
											{
												set = value2;
											}
											else
											{
												continue;
											}
											two[0].Set(set);
											two[1].Set(set);
											break;
										}
									}

									puzzle.LogAction(Puzzle.TechniqueFormat("Unique rectangle", "{0}: {1}", cells.Print(), candidates.Print()), cells, (Cell?)null);
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

	private static bool XYChain(Puzzle puzzle)
	{
		bool Recursion(Cell startCell, List<Cell> ignore, Cell currentCell, int theOneThatWillEndItAllBaybee, int mustFind)
		{
			ignore.Add(currentCell);
			IEnumerable<Cell> visible = currentCell.VisibleCells.Except(ignore);
			foreach (Cell cell in visible)
			{
				if (cell.Candidates.Count != 2)
				{
					continue; // Must have two candidates
				}
				if (!cell.Candidates.Contains(mustFind))
				{
					continue; // Must have "mustFind"
				}

				int otherCandidate = cell.Candidates.Except(new int[] { mustFind }).Single();
				// Check end condition
				if (otherCandidate == theOneThatWillEndItAllBaybee && startCell != currentCell)
				{
					Cell[] commonVisibleWithStartCell = cell.VisibleCells.Intersect(startCell.VisibleCells).ToArray();
					if (commonVisibleWithStartCell.Length > 0)
					{
						IEnumerable<Cell> commonWithEndingCandidate = commonVisibleWithStartCell.Where(c => c.Candidates.Contains(theOneThatWillEndItAllBaybee));
						if (Cell.ChangeCandidates(commonWithEndingCandidate, theOneThatWillEndItAllBaybee))
						{
							ignore.Remove(startCell); // Remove here because we're now using "ignore" as "semiCulprits" and exiting
							var culprits = new Cell[] { startCell, cell };
							puzzle.LogAction(Puzzle.TechniqueFormat("XY-Chain", "{0}-{1}: {2}", culprits.Print(), ignore.SingleOrMultiToString(), theOneThatWillEndItAllBaybee), culprits, ignore);
							return true;
						}
					}
				}
				// Loop again
				if (Recursion(startCell, ignore, cell, theOneThatWillEndItAllBaybee, otherCandidate))
				{
					return true;
				}
			}
			ignore.Remove(currentCell);
			return false;
		}

		for (int x = 0; x < 9; x++)
		{
			for (int y = 0; y < 9; y++)
			{
				Cell cell = puzzle[x, y];
				if (cell.Candidates.Count != 2)
				{
					continue; // Must have two candidates
				}
				var ignore = new List<Cell>();
				int start1 = cell.Candidates.ElementAt(0);
				int start2 = cell.Candidates.ElementAt(1);
				if (Recursion(cell, ignore, cell, start1, start2) || Recursion(cell, ignore, cell, start2, start1))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool XYZWing(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			bool FindXYZWings(Region region)
			{
				bool changed = false;
				Cell[] cells2 = region.Where(c => c.Candidates.Count == 2).ToArray();
				Cell[] cells3 = region.Where(c => c.Candidates.Count == 3).ToArray();
				if (cells2.Length > 0 && cells3.Length > 0)
				{
					for (int j = 0; j < cells2.Length; j++)
					{
						Cell c2 = cells2[j];
						for (int k = 0; k < cells3.Length; k++)
						{
							Cell c3 = cells3[k];
							if (c2.Candidates.Intersect(c3.Candidates).Count() != 2)
							{
								continue;
							}

							IEnumerable<Cell> c3Sees = c3.VisibleCells.Except(region)
								.Where(c => c.Candidates.Count == 2 // If it has 2 candidates
								&& c.Candidates.Intersect(c3.Candidates).Count() == 2 // Shares them both with p3
								&& c.Candidates.Intersect(c2.Candidates).Count() == 1); // And shares one with p2
							foreach (Cell c2_2 in c3Sees)
							{
								IEnumerable<Cell> allSee = c2.VisibleCells.Intersect(c3.VisibleCells).Intersect(c2_2.VisibleCells);
								int allHave = c2.Candidates.Intersect(c3.Candidates).Intersect(c2_2.Candidates).Single(); // Will be 1 Length
								if (Cell.ChangeCandidates(allSee, allHave))
								{
									var culprits = new Cell[] { c2, c3, c2_2 };
									puzzle.LogAction(Puzzle.TechniqueFormat("XYZ-Wing", "{0}: {1}", culprits.Print(), allHave), culprits, (Cell?)null);
									changed = true;
								}
							}
						}
					}
				}
				return changed;
			}

			if (FindXYZWings(puzzle.Rows[i]) || FindXYZWings(puzzle.Columns[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool YWing(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			bool FindYWings(Region region)
			{
				Cell[] cells = region.Where(c => c.Candidates.Count == 2).ToArray();
				if (cells.Length > 1)
				{
					for (int j = 0; j < cells.Length; j++)
					{
						Cell c1 = cells[j];
						for (int k = j + 1; k < cells.Length; k++)
						{
							Cell c2 = cells[k];
							IEnumerable<int> inter = c1.Candidates.Intersect(c2.Candidates);
							if (inter.Count() != 1)
							{
								continue;
							}

							int other1 = c1.Candidates.Except(inter).ElementAt(0),
									other2 = c2.Candidates.Except(inter).ElementAt(0);

							var a = new Cell[] { c1, c2 };
							foreach (Cell cell in a)
							{
								IEnumerable<Cell> c3a = cell.VisibleCells.Except(cells).Where(c => c.Candidates.Count == 2 && c.Candidates.Intersect(new int[] { other1, other2 }).Count() == 2);
								if (c3a.Count() == 1) // Example: p1 and p3 see each other, so remove similarities from p2 and p3
								{
									Cell c3 = c3a.ElementAt(0);
									Cell cOther = a.Single(c => c != cell);
									IEnumerable<Cell> commonCells = cOther.VisibleCells.Intersect(c3.VisibleCells);
									int candidate = cOther.Candidates.Intersect(c3.Candidates).Single(); // Will just be 1 candidate
									if (Cell.ChangeCandidates(commonCells, candidate))
									{
										var culprits = new Cell[] { c1, c2, c3 };
										puzzle.LogAction(Puzzle.TechniqueFormat("Y-Wing", "{0}: {1}", culprits.Print(), candidate), culprits, (Cell?)null);
										return true;
									}
								}
							}
						}
					}
				}
				return false;
			}

			if (FindYWings(puzzle.Rows[i]) || FindYWings(puzzle.Columns[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool Jellyfish(Puzzle puzzle)
	{
		return FindFish(puzzle, 4);
	}

	private static bool Swordfish(Puzzle puzzle)
	{
		return FindFish(puzzle, 3);
	}

	private static bool XWing(Puzzle puzzle)
	{
		return FindFish(puzzle, 2);
	}

	private static bool PointingTuple(Puzzle puzzle)
	{
		for (int i = 0; i < 3; i++)
		{
			var blockrow = new Cell[3][];
			var blockcol = new Cell[3][];
			for (int r = 0; r < 3; r++)
			{
				blockrow[r] = puzzle.Blocks[r + (i * 3)].ToArray();
				blockcol[r] = puzzle.Blocks[i + (r * 3)].ToArray();
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
						Cell[] culprits = doRows ? blockrow[r].GetRowInBlock(rcIndex) : blockcol[r].GetColumnInBlock(rcIndex);
						puzzle.LogAction(Puzzle.TechniqueFormat("Pointing tuple",
							"Starting in block{0} {1}'s {2} block, {3} {0}: {4}",
							doRows ? "row" : "column", i + 1, _ordinalStr[r + 1], _ordinalStr[rcIndex + 1], candidates.SingleOrMultiToString()), culprits, (Cell?)null);
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

	private static bool LockedCandidate(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			for (int candidate = 1; candidate <= 9; candidate++)
			{
				bool FindLockedCandidates(bool doRows)
				{
					IEnumerable<Cell> cellsWithCandidates = (doRows ? puzzle.Rows : puzzle.Columns)[i].GetCellsWithCandidate(candidate);

					// Even if a block only has these candidates for this "k" value, it'd be slower to check that before cancelling "BlacklistCandidates"
					if (cellsWithCandidates.Count() == 3 || cellsWithCandidates.Count() == 2)
					{
						int[] blocks = cellsWithCandidates.Select(c => c.Point.BlockIndex).Distinct().ToArray();
						if (blocks.Length == 1)
						{
							if (Cell.ChangeCandidates(puzzle.Blocks[blocks[0]].Except(cellsWithCandidates), candidate))
							{
								puzzle.LogAction(Puzzle.TechniqueFormat("Locked candidate",
									"{4} {0} locks within block {1}: {2}: {3}",
									doRows ? SPoint.RowLetter(i) : SPoint.ColumnLetter(i), blocks[0] + 1, cellsWithCandidates.Print(), candidate, doRows ? "Row" : "Column"), cellsWithCandidates, (Cell?)null);
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

	private static bool HiddenQuadruple(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 4)
				|| FindHiddenTuples(puzzle, puzzle.Rows[i], 4)
				|| FindHiddenTuples(puzzle, puzzle.Columns[i], 4))
			{
				return true;
			}
		}
		return false;
	}

	private static bool NakedQuadruple(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			if (FindNakedTuples(puzzle, puzzle.Blocks[i], 4)
				|| FindNakedTuples(puzzle, puzzle.Rows[i], 4)
				|| FindNakedTuples(puzzle, puzzle.Columns[i], 4))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HiddenTriple(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 3)
				|| FindHiddenTuples(puzzle, puzzle.Rows[i], 3)
				|| FindHiddenTuples(puzzle, puzzle.Columns[i], 3))
			{
				return true;
			}
		}
		return false;
	}

	private static bool NakedTriple(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			if (FindNakedTuples(puzzle, puzzle.Blocks[i], 3)
				|| FindNakedTuples(puzzle, puzzle.Rows[i], 3)
				|| FindNakedTuples(puzzle, puzzle.Columns[i], 3))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HiddenPair(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			if (FindHiddenTuples(puzzle, puzzle.Blocks[i], 2)
				|| FindHiddenTuples(puzzle, puzzle.Rows[i], 2)
				|| FindHiddenTuples(puzzle, puzzle.Columns[i], 2))
			{
				return true;
			}
		}
		return false;
	}

	private static bool NakedPair(Puzzle puzzle)
	{
		for (int i = 0; i < 9; i++)
		{
			if (FindNakedTuples(puzzle, puzzle.Blocks[i], 2)
				|| FindNakedTuples(puzzle, puzzle.Rows[i], 2)
				|| FindNakedTuples(puzzle, puzzle.Columns[i], 2))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HiddenSingle(Puzzle puzzle)
	{
		bool changed = false;
		for (int i = 0; i < 9; i++)
		{
			foreach (ReadOnlyCollection<Region> region in puzzle.Regions)
			{
				for (int candidate = 1; candidate <= 9; candidate++)
				{
					Cell[] c = region[i].GetCellsWithCandidate(candidate).ToArray();
					if (c.Length == 1)
					{
						c[0].Set(candidate);
						puzzle.LogAction(Puzzle.TechniqueFormat("Hidden single", "{0}: {1}", c[0], candidate), c[0], (Cell?)null);
						changed = true;
					}
				}
			}
		}
		return changed;
	}
}
