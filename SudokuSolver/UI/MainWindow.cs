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

        void LogList_SelectedIndexChanged(object sender, EventArgs e) => sudokuBoard.ReDraw(true, logList.SelectedIndex);
        void ChangeState(bool solveButtonState, bool saveState)
        {
            solveButton.Enabled = solveButtonState;
            saveAsToolStripMenuItem.Enabled = saveState;
        }

        void ChangePuzzle(string name, bool buttonState)
        {
            ChangeState(buttonState, false);
            puzzleLabel.Text = name + " Puzzle";
            statusLabel.Text = "";
            logList.DataSource = Logger.Actions = new BindingList<string>();
            sudokuBoard.SetBoard(solver.Puzzle);
        }
        void NewPuzzle(object sender, EventArgs e)
        {
            solver = new Solver(Utils.CreateJaggedArray<int[][]>(9, 9), true);
            ChangePuzzle("Custom", false);
            Logger.Log("Custom puzzle created");
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
            if (d.ShowDialog() != DialogResult.OK) return;

            if (Puzzle.Load(d.FileName, out solver))
                ChangePuzzle(Path.GetFileNameWithoutExtension(d.FileName), true);
            else
                MessageBox.Show("Invalid puzzle data.");
        }
        void SavePuzzle(object sender, EventArgs e)
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
        void SolvePuzzle(object sender, EventArgs e)
        {
            ChangeState(false, saveAsToolStripMenuItem.Enabled);
            if (solver.Puzzle.IsCustom) // This check goes here so the solver isn't slowed down. The solver on its own would not have to do this
                for (int x = 0; x < 9; x++)
                    for (int y = 0; y < 9; y++)
                        if (solver.Puzzle[x, y] != solver.Puzzle[x, y].OriginalValue)
                            solver.Puzzle[x, y].Set(0);
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
            Logger.Log(string.Format("Solver {0} the puzzle", ((bool)e.Result) ? "completed" : "failed"));
            logList.SelectedIndex = Logger.Actions.Count - 1;
            logList.Select();
        }

        void Exit(object sender, EventArgs e) => Environment.Exit(0);
    }
}
