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
            statusLabel.Text = "";
            logList.SelectedIndexChanged += LogList_SelectedIndexChanged;
        }

        private void LogList_SelectedIndexChanged(object sender, EventArgs e) => sudokuBoard.ReDraw(true, logList.SelectedIndex);

        private void SolveButton_Click(object sender, EventArgs e)
        {
            solveButton.Enabled = false;
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
            var log = (string[])e.Result;
            logList.DataSource = log;
            logList.SelectedIndex = log.Length - 1;
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

            solver = new Solver(board, sudokuBoard);
            return true;
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
            {
                solveButton.Enabled = true;
                statusLabel.Text = "";
                logList.SelectedIndexChanged -= LogList_SelectedIndexChanged;
                logList.DataSource = null;
                logList.SelectedIndexChanged += LogList_SelectedIndexChanged;
            }
            else
            {
                MessageBox.Show("Invalid puzzle data.");
            }
        }
    }
}
