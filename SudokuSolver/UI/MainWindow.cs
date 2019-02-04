using Kermalis.SudokuSolver.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Kermalis.SudokuSolver.UI
{
    partial class MainWindow : Form
    {
        Stopwatch stopwatch;
        Solver solver;

        public MainWindow()
        {
            InitializeComponent();
            puzzleLabel.Text = "";
            statusLabel.Text = "";
            logList.SelectedIndexChanged += LogList_SelectedIndexChanged;
            sudokuBoard.CellChanged += (cell) => ChangeState(true, true);
        }

        void LogList_SelectedIndexChanged(object sender, EventArgs e)
        {
            sudokuBoard.ReDraw(true, logList.SelectedIndex);
        }
        void ChangeState(bool solveButtonState, bool saveState)
        {
            solveButton.Enabled = solveButtonState;
            saveAsToolStripMenuItem.Enabled = saveState;
        }

        void ChangePuzzle(string puzzleName, bool solveButtonState)
        {
            ChangeState(solveButtonState, false);
            puzzleLabel.Text = puzzleName + " Puzzle";
            statusLabel.Text = "";
            logList.DataSource = solver.Puzzle.Actions;
            sudokuBoard.SetBoard(solver.Puzzle);
        }
        void NewPuzzle(object sender, EventArgs e)
        {
            solver = new Solver(new Puzzle(Utils.CreateJaggedArray<int[][]>(9, 9), true));
            ChangePuzzle("Custom", false);
            solver.Puzzle.LogAction("Custom puzzle created");
            MessageBox.Show("A custom puzzle has been created. Click cells to type in values.", Text);
        }
        void OpenPuzzle(object sender, EventArgs e)
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
                solver = new Solver(puzzle);
                ChangePuzzle(Path.GetFileNameWithoutExtension(d.FileName), true);
            }
        }
        void SavePuzzle(object sender, EventArgs e)
        {
            var d = new SaveFileDialog
            {
                Title = "Save Sudoku Puzzle",
                Filter = "TXT files|*.txt",
                InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\Puzzles")
            };
            if (d.ShowDialog() == DialogResult.OK)
            {
                solver.Puzzle.Save(d.FileName);
                MessageBox.Show("Puzzle saved.", Text);
            }
        }
        void SolvePuzzle(object sender, EventArgs e)
        {
            ChangeState(false, saveAsToolStripMenuItem.Enabled);
            // Clear solver's guesses on a custom puzzle
            if (solver.Puzzle.IsCustom)
            {
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        if (solver.Puzzle[x, y].Value != solver.Puzzle[x, y].OriginalValue)
                        {
                            solver.Puzzle[x, y].Set(0);
                        }
                    }
                }
            }
            stopwatch = new Stopwatch();
            var bw = new BackgroundWorker();
            bw.DoWork += solver.DoWork;
            bw.RunWorkerCompleted += SolverFinished;
            stopwatch.Start();
            bw.RunWorkerAsync();
        }
        void SolverFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();
            statusLabel.Text = string.Format("Solver finished in {0} seconds.", stopwatch.Elapsed.TotalSeconds);
            solver.Puzzle.LogAction(string.Format("Solver {0} the puzzle", ((bool)e.Result) ? "completed" : "failed"));
            logList.SelectedIndex = solver.Puzzle.Actions.Count - 1;
            logList.Select();
        }

        void Exit(object sender, EventArgs e)
        {
            Close();
        }
    }
}
