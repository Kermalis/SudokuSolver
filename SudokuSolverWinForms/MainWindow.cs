using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Kermalis.SudokuSolver.UI;

internal sealed partial class MainWindow : Form // TODO: Readme, alloc, tests, fix custom puzzle
{
	private Solver? _solver;

	public MainWindow()
	{
		InitializeComponent();

		_puzzleLabel.Text = string.Empty;
		_statusLabel.Text = string.Empty;
		_logList.SelectedIndexChanged += LogList_SelectedIndexChanged;
		_sudokuBoard.CellChanged += (cell) => ChangeState(true, true);
	}

	private void LogList_SelectedIndexChanged(object? sender, EventArgs e)
	{
		_sudokuBoard.ReDraw(true, _logList.SelectedIndex);
	}

	private void ChangeState(bool solveButtonState, bool saveState)
	{
		_solveButton.Enabled = solveButtonState;
		_saveAsToolStripMenuItem.Enabled = saveState;
	}

	private void ChangePuzzle(Solver newSolver, string puzzleName, bool solveButtonState)
	{
		_solver = newSolver;
		ChangeState(solveButtonState, false);
		_puzzleLabel.Text = puzzleName + " Puzzle";
		_statusLabel.Text = string.Empty;
		_logList.DataSource = _solver.Actions;
		_sudokuBoard.SetSolver(_solver);
	}

	private void NewPuzzle(object? sender, EventArgs e)
	{
		ChangePuzzle(Solver.CreateCustomPuzzle(), "Custom", false);
		MessageBox.Show("A custom puzzle has been created. Click cells to type in values.", Text);
	}

	private void OpenPuzzle(object? sender, EventArgs e)
	{
		var d = new OpenFileDialog
		{
			Title = "Open Sudoku Puzzle",
			Filter = "TXT files|*.txt",
			InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + @"\..\Puzzles")
		};

		if (d.ShowDialog() == DialogResult.OK)
		{
			Puzzle puzzle;
			try
			{
				puzzle = Puzzle.Parse(File.ReadAllLines(d.FileName));
			}
			catch (InvalidDataException)
			{
				MessageBox.Show("Invalid puzzle data.");
				return;
			}
			catch
			{
				MessageBox.Show("Error loading puzzle.");
				return;
			}

			ChangePuzzle(new Solver(puzzle), Path.GetFileNameWithoutExtension(d.FileName), true);
		}
	}

	private void SavePuzzle(object? sender, EventArgs e)
	{
		var d = new SaveFileDialog
		{
			Title = "Save Sudoku Puzzle",
			Filter = "TXT files|*.txt",
			InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + @"\..\Puzzles")
		};

		if (d.ShowDialog() == DialogResult.OK)
		{
			File.WriteAllText(d.FileName, _solver!.Puzzle.ToString());
			MessageBox.Show("Puzzle saved.", Text);
		}
	}

	private void SolvePuzzle(object? sender, EventArgs e)
	{
		ChangeState(false, _saveAsToolStripMenuItem.Enabled);
		// Clear solver's guesses on a custom puzzle
		if (_solver!.Puzzle.IsCustom)
		{
			_solver.Puzzle.Reset();
		}

		var sw = new Stopwatch();
		sw.Start();
		_solver.TrySolve();
		sw.Stop();
		_statusLabel.Text = string.Format("Solver finished in {0} seconds.", sw.Elapsed.TotalSeconds);
		_logList.SelectedIndex = _solver.Actions.Count - 1;
		_logList.Select();
	}

	private void Exit(object? sender, EventArgs e)
	{
		Close();
	}
}
