using SudokuSolver.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace SudokuSolver
{
    public partial class MainWindow : Form
    {
        Solver solver;

        public MainWindow()
        {
            InitializeComponent();
            solveButton.Enabled = false;
            toolStripStatusLabel1.Text = "";
            /*byte[,] board = new byte[9, 9] {
            { 6, 0, 0,     9, 0, 0,     0, 0, 0 },
            { 0, 3, 0,     4, 0, 5,     0, 1, 0 },
            { 7, 0, 8,     0, 1, 0,     5, 0, 0 },

            { 9, 1, 7,     0, 6, 0,     4, 0, 0 },
            { 0, 0, 0,     0, 4, 0,     0, 0, 0 },
            { 0, 0, 4,     0, 8, 0,     9, 3, 2 },

            { 0, 0, 5,     0, 2, 0,     8, 0, 6 },
            { 0, 7, 0,     1, 0, 4,     0, 2, 0 },
            { 0, 0, 0,     0, 0, 6,     0, 0, 1 }}; // Solves
            solver = new Solver(sudokuBoard1, board.ToJaggedArray());*/
        }

        private void SolveButton_Click(object sender, EventArgs e)
        {
            solveButton.Enabled = false;
            var sw = new Stopwatch();
            sw.Start();
            solver.Begin();
            sw.Stop();
            toolStripStatusLabel1.Text = string.Format("Solver finished in {0}.{1:D3} seconds.", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds % 1000);
        }

        // Return true if puzzle was loaded correctly
        private bool LoadPuzzle(string filename)
        {
            string[] filelines = File.ReadAllLines(filename);
            if (filelines.Length != 9) return false;
            var board = Utils.CreateJaggedArray<byte[][]>(9,9);
            for (byte i = 0; i < 9; i++)
            {
                string[] split = filelines[i].Split(',');
                if (split.Length != 9) return false;
                for (byte j = 0; j < 9; j++)
                {
                    if (!string.IsNullOrEmpty(split[j]))
                    {
                        if (split[j].Length > 1) return false;
                        board[j][i] = byte.Parse(split[j]);
                    }
                }
            }

            solver = new Solver(sudokuBoard1, board);
            return true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog
            {
                Title = "Open Sudoku Puzzle",
                Filter = "TXT files|*.txt",
                InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\Puzzles")
            };
            if (d.ShowDialog() == DialogResult.OK)
            {
                if (LoadPuzzle(d.FileName))
                {
                    solveButton.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Invalid puzzle data.");
                    return;
                }
            }
        }
    }
}
