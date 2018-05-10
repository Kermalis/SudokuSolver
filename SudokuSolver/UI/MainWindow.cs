using SudokuSolver.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SudokuSolver
{
    public partial class MainWindow : Form
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

        private void LogList_SelectedIndexChanged(object sender, EventArgs e) => sudokuBoard.ReDraw(true, logList.SelectedIndex);
        private void ChangeState(bool solveButtonState, bool saveState)
        {
            solveButton.Enabled = solveButtonState;
            saveAsToolStripMenuItem.Enabled = saveState;
        }

        private void ChangePuzzle(string name, bool buttonState)
        {
            ChangeState(buttonState, false);
            puzzleLabel.Text = name + " Puzzle";
            statusLabel.Text = "";
            logList.SelectedIndexChanged -= LogList_SelectedIndexChanged;
            logList.DataSource = null;
            logList.SelectedIndexChanged += LogList_SelectedIndexChanged;
            sudokuBoard.SetBoard(solver.Puzzle);
        }
        private void NewPuzzle(object sender, EventArgs e)
        {
            solver = new Solver(Utils.CreateJaggedArray<int[][]>(9, 9), true);
            ChangePuzzle("Custom", false);
            logList.DataSource = solver.Puzzle.Actions;
            solver.Puzzle.Log("Custom puzzle created");
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
            if (d.ShowDialog() != DialogResult.OK) return;

            if (Puzzle.Load(d.FileName, out solver))
                ChangePuzzle(Path.GetFileNameWithoutExtension(d.FileName), true);
            else
                MessageBox.Show("Invalid puzzle data.");
        }
        private void SavePuzzle(object sender, EventArgs e)
        {
            var d = new SaveFileDialog
            {
                Title = "Save Sudoku Puzzle",
                Filter = "TXT files|*.txt",
                InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\Puzzles")
            };
            if (d.ShowDialog() != DialogResult.OK) return;

            solver.Puzzle.Save(d.FileName);
            MessageBox.Show("Puzzle saved.", Text);
        }
        private void SolvePuzzle(object sender, EventArgs e)
        {
            ChangeState(false, saveAsToolStripMenuItem.Enabled);
            stopwatch = new Stopwatch();
            var bw = new BackgroundWorker();
            bw.DoWork += solver.DoWork;
            bw.RunWorkerCompleted += SolverFinished;
            stopwatch.Start();
            bw.RunWorkerAsync();
        }
        private void SolverFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();
            logList.DataSource = null; // If a new puzzle is used it glitches out for some reason unless I put this
            solver.Puzzle.Log("Solver {0} the puzzle", ((bool)e.Result) ? "completed" : "failed");
            logList.DataSource = solver.Puzzle.Actions;
            logList.SelectedIndex = solver.Puzzle.Actions.Count - 1;
            logList.Select();
            statusLabel.Text = string.Format("Solver finished in {0} seconds.", stopwatch.Elapsed.TotalSeconds);
        }
    }
}
