using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Kermalis.SudokuSolver;

public sealed partial class Solver
{
	public Puzzle Puzzle { get; }
	public BindingList<PuzzleSnapshot> Actions { get; }

	public Solver(Puzzle puzzle)
	{
		Puzzle = puzzle;
		Actions = [];

		_techniques = InitSolverTechniques();
	}
	public static Solver CreateCustomPuzzle()
	{
		var s = new Solver(Puzzle.CreateCustom());
		s.LogAction("Custom puzzle created");
		return s;
	}

	public void SetOriginalCellValue(Cell cell, int value)
	{
		if (cell.Puzzle != Puzzle)
		{
			throw new ArgumentOutOfRangeException(nameof(cell), cell, "Cell belongs to another puzzle");
		}

		cell.ChangeOriginalValue(value);
		LogAction(TechniqueFormat("Changed cell", cell.ToString()),
			cell);
	}

	public bool TrySolve()
	{
		Puzzle.RefreshCandidates();
		LogAction("Begin");

		do
		{
			if (CheckForNakedSinglesOrCompletion(out bool changed))
			{
				LogAction("Solver completed the puzzle");
				return true;
			}
			if (changed)
			{
				continue;
			}
			if (!RunTechnique())
			{
				LogAction("Solver failed the puzzle");
				return false;
			}
		} while (true);
	}
	public bool TrySolveAsync(CancellationToken ct)
	{
		Puzzle.RefreshCandidates();
		LogAction("Begin");

		do
		{
			if (CheckForNakedSinglesOrCompletion(out bool changed))
			{
				LogAction("Solver completed the puzzle");
				return true;
			}
			if (ct.IsCancellationRequested)
			{
				break;
			}
			if (changed)
			{
				continue;
			}
			if (!RunTechnique())
			{
				LogAction("Solver failed the puzzle");
				return false;
			}
			if (ct.IsCancellationRequested)
			{
				break;
			}
		} while (true);

		LogAction("Solver cancelled");
		throw new OperationCanceledException(ct);
	}
	private bool CheckForNakedSinglesOrCompletion(out bool changed)
	{
		changed = false;
		bool solved = true;
	again:
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				if (cell.Value != Cell.EMPTY_VALUE)
				{
					continue;
				}

				// Empty cell... check for naked single
				solved = false;
				if (cell.CandI.TryGetCount1(out int nakedSingle))
				{
					cell.SetValue(nakedSingle);

					LogAction(TechniqueFormat("Naked single",
						"{0}: {1}",
						cell, nakedSingle),
						cell);

					changed = true;
					goto again; // Restart the search for naked singles since we have the potential to create new ones
				}
			}
		}
		return solved;
	}

	private bool RunTechnique()
	{
		foreach (SolverTechnique t in _techniques)
		{
			if (t.Function.Invoke())
			{
				return true;
			}
		}
		return false;
	}

	private static string TechniqueFormat(string technique, string format, params object[] args)
	{
		return string.Format(string.Format("{0,-20}", technique) + format, args);
	}
	private void LogAction(string action)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, false, false);
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
	private void LogAction(string action, Cell culprit)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, culprit == cell, false);
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
	private void LogAction(string action, Cell culprit, Cell semiCulprit)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, culprit == cell, semiCulprit == cell);
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
	private void LogAction(string action, ReadOnlySpan<Cell> culprits)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, culprits.SimpleIndexOf(cell) != -1, false);
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
	private void LogAction(string action, ReadOnlySpan<Cell> culprits, Cell semiCulprit)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, culprits.SimpleIndexOf(cell) != -1, semiCulprit == cell);
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
	private void LogAction(string action, Cell culprit, ReadOnlySpan<Cell> semiCulprits)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, culprit == cell, semiCulprits.SimpleIndexOf(cell) != -1);
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
	private void LogAction(string action, ReadOnlySpan<Cell> culprits, ReadOnlySpan<Cell> semiCulprits)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, culprits.SimpleIndexOf(cell) != -1, semiCulprits.SimpleIndexOf(cell) != -1);
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
	public void LogAction(string action, IEnumerable<Cell> culprits)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, culprits.Contains(cell), false);
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
	public void LogAction(string action, IEnumerable<Cell> culprits, IEnumerable<Cell> semiCulprits)
	{
		var sBoard = new CellSnapshot[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];

				sBoard[Utils.CellIndex(col, row)] = new CellSnapshot(cell, culprits.Contains(cell), semiCulprits.Contains(cell));
			}
		}
		Actions.Add(new PuzzleSnapshot(action, sBoard));
	}
}