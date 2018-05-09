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
            solveButton.Enabled = false;
            puzzleLabel.Text = "";
            statusLabel.Text = "";
            logList.SelectedIndexChanged += LogList_SelectedIndexChanged;
            sudokuBoard.CellChanged += (cell) => ChangeSolveButtonState(true);
        }

        private void LogList_SelectedIndexChanged(object sender, EventArgs e) => sudokuBoard.ReDraw(true, logList.SelectedIndex);

        private void ChangeSolveButtonState(bool state) => solveButton.Enabled = state;

        private void SolveButton_Click(object sender, EventArgs e)
        {
            ChangeSolveButtonState(false);
            stopwatch = new Stopwatch();
            var bw = new BackgroundWorker();
            bw.DoWork += solver.DoWork;
            bw.RunWorkerCompleted += Solver_Finished;
            stopwatch.Start();
            bw.RunWorkerAsync();
        }

        private void Solver_Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            stopwatch.Stop();
            var board = (Board)e.Result;
            logList.DataSource = null; // If a new puzzle is used it glitches out for some reason unless I put this
            logList.DataSource = board.Actions;
            logList.SelectedIndex = board.Actions.Count - 1;
            logList.Select();
            statusLabel.Text = string.Format("Solver finished in {0} seconds.", stopwatch.Elapsed.TotalSeconds);
        }

        // Return true if puzzle was loaded correctly
        private bool LoadPuzzle(string filename)
        {
            string[] filelines = File.ReadAllLines(filename);
            if (filelines.Length != 9) return false;
            var board = Utils.CreateJaggedArray<int[][]>(9, 9);
            for (int i = 0; i < 9; i++)
            {
                string line = filelines[i];
                if (line.Length != 9) return false;
                for (int j = 0; j < 9; j++)
                {
                    if (byte.TryParse(line[j].ToString(), out byte value)) // Anything can represent 0
                    {
                        board[j][i] = value;
                    }
                }
            }

            solver = new Solver(board, false, out Board b);
            sudokuBoard.SetBoard(b);
            return true;
        }

        private void ChangePuzzle(string name, bool buttonState)
        {
            ChangeSolveButtonState(buttonState);
            puzzleLabel.Text = name + " Puzzle";
            statusLabel.Text = "";
            logList.SelectedIndexChanged -= LogList_SelectedIndexChanged;
            logList.DataSource = null;
            logList.SelectedIndexChanged += LogList_SelectedIndexChanged;
        }

        private void OpenPuzzle(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog
            {
                Title = "Open Sudoku Puzzle",
                Filter = "TXT files|*.txt",
                InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\Puzzles")
            };
            if (d.ShowDialog() != DialogResult.OK) return;

            if (LoadPuzzle(d.FileName))
                ChangePuzzle(Path.GetFileNameWithoutExtension(d.FileName), true);
            else
                MessageBox.Show("Invalid puzzle data.");
        }

        private void NewPuzzle(object sender, EventArgs e)
        {
            ChangePuzzle("Custom", false);
            solver = new Solver(Utils.CreateJaggedArray<int[][]>(9, 9), true, out Board board);
            sudokuBoard.SetBoard(board);
            logList.DataSource = board.Actions;
            board.Log("Custom puzzle created");
            MessageBox.Show("A custom puzzle has been created. Click cells to type in values.", "Custom Puzzle");
        }
    }
}
