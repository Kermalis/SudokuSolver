using Kermalis.SudokuSolver.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Kermalis.SudokuSolver.UI
{
    internal sealed partial class MainWindow : Form
    {
        private Stopwatch _stopwatch;
        private Solver _solver;

        public MainWindow()
        {
            InitializeComponent();
            _puzzleLabel.Text = string.Empty;
            _statusLabel.Text = string.Empty;
            _logList.SelectedIndexChanged += LogList_SelectedIndexChanged;
            _sudokuBoard.CellChanged += (cell) => ChangeState(true, true);
        }

        private void LogList_SelectedIndexChanged(object sender, EventArgs e)
        {
            _sudokuBoard.ReDraw(true, _logList.SelectedIndex);
        }

        private void ChangeState(bool solveButtonState, bool saveState)
        {
            _solveButton.Enabled = solveButtonState;
            _saveAsToolStripMenuItem.Enabled = saveState;
        }

        private void ChangePuzzle(string puzzleName, bool solveButtonState)
        {
            ChangeState(solveButtonState, false);
            _puzzleLabel.Text = puzzleName + " Puzzle";
            _statusLabel.Text = string.Empty;
            _logList.DataSource = _solver.Puzzle.Actions;
            _sudokuBoard.SetBoard(_solver.Puzzle);
        }

        private void NewPuzzle(object sender, EventArgs e)
        {
            _solver = new Solver(new Puzzle(Utils.CreateJaggedArray<int[][]>(9, 9), true));
            ChangePuzzle("Custom", false);
            _solver.Puzzle.LogAction("Custom puzzle created");
            MessageBox.Show("A custom puzzle has been created. Click cells to type in values.", Text);
        }

        private void OpenPuzzle(object sender, EventArgs e)
        {
            var d = new OpenFileDialog
            {
                Title = "Open Sudoku Puzzle",
                Filter = "TXT files|*.txt",
                InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\Puzzles")
            };
            if (d.ShowDialog() == DialogResult.OK)
            {
                Puzzle puzzle;
                try
                {
                    puzzle = Puzzle.Load(d.FileName);
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
                _solver = new Solver(puzzle);
                ChangePuzzle(Path.GetFileNameWithoutExtension(d.FileName), true);
            }
        }

        private void SavePuzzle(object sender, EventArgs e)
        {
            var d = new SaveFileDialog
            {
                Title = "Save Sudoku Puzzle",
                Filter = "TXT files|*.txt",
                InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\Puzzles")
            };
            if (d.ShowDialog() == DialogResult.OK)
            {
                _solver.Puzzle.Save(d.FileName);
                MessageBox.Show("Puzzle saved.", Text);
            }
        }

        private void SolvePuzzle(object sender, EventArgs e)
        {
            ChangeState(false, _saveAsToolStripMenuItem.Enabled);
            // Clear solver's guesses on a custom puzzle
            if (_solver.Puzzle.IsCustom)
            {
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        if (_solver.Puzzle[x, y].Value != _solver.Puzzle[x, y].OriginalValue)
                        {
                            _solver.Puzzle[x, y].Set(0);
                        }
                    }
                }
            }
            _stopwatch = new Stopwatch();
            var bw = new BackgroundWorker();
            bw.DoWork += _solver.DoWork;
            bw.RunWorkerCompleted += SolverFinished;
            _stopwatch.Start();
            bw.RunWorkerAsync();
        }

        private void SolverFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            _stopwatch.Stop();
            _statusLabel.Text = string.Format("Solver finished in {0} seconds.", _stopwatch.Elapsed.TotalSeconds);
            _solver.Puzzle.LogAction(string.Format("Solver {0} the puzzle", ((bool)e.Result) ? "completed" : "failed"));
            _logList.SelectedIndex = _solver.Puzzle.Actions.Count - 1;
            _logList.Select();
        }

        private void Exit(object sender, EventArgs e)
        {
            Close();
        }
    }
}
