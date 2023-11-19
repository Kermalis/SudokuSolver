using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Kermalis.SudokuSolver;

public sealed partial class Solver
{
	public Puzzle Puzzle { get; }
	public BindingList<string> Actions { get; }

	public Solver(Puzzle puzzle)
	{
		Puzzle = puzzle;
		Actions = new BindingList<string>();

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
		string action = TechniqueFormat("Changed cell", cell.ToString());
		LogAction(action, cell);
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
				if (cell.Candidates.TryGetCount1(out int nakedSingle))
				{
					cell.Set(nakedSingle);

					string action = TechniqueFormat("Naked single", "{0}: {1}",
						cell, nakedSingle);
					LogAction(action, cell);

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
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(false, false);
			}
		}
		Actions.Add(action);
	}
	private void LogAction(string action, Cell culprit)
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(culprit == cell, false);
			}
		}
		Actions.Add(action);
	}
	private void LogAction(string action, Cell culprit, Cell semiCulprit)
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(culprit == cell, semiCulprit == cell);
			}
		}
		Actions.Add(action);
	}
	private void LogAction(string action, ReadOnlySpan<Cell> culprits)
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(culprits.SimpleIndexOf(cell) != -1, false);
			}
		}
		Actions.Add(action);
	}
	private void LogAction(string action, ReadOnlySpan<Cell> culprits, Cell semiCulprit)
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(culprits.SimpleIndexOf(cell) != -1, semiCulprit == cell);
			}
		}
		Actions.Add(action);
	}
	private void LogAction(string action, Cell culprit, ReadOnlySpan<Cell> semiCulprits)
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(culprit == cell, semiCulprits.SimpleIndexOf(cell) != -1);
			}
		}
		Actions.Add(action);
	}
	private void LogAction(string action, ReadOnlySpan<Cell> culprits, ReadOnlySpan<Cell> semiCulprits)
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(culprits.SimpleIndexOf(cell) != -1, semiCulprits.SimpleIndexOf(cell) != -1);
			}
		}
		Actions.Add(action);
	}
	public void LogAction(string action, IEnumerable<Cell> culprits)
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(culprits.Contains(cell), false);
			}
		}
		Actions.Add(action);
	}
	public void LogAction(string action, IEnumerable<Cell> culprits, IEnumerable<Cell> semiCulprits)
	{
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				Cell cell = Puzzle[col, row];
				cell.CreateSnapshot(culprits.Contains(cell), semiCulprits.Contains(cell));
			}
		}
		Actions.Add(action);
	}
}
