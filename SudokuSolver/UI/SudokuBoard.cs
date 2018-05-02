using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuSolver.UI
{
    public class SudokuBoard : UserControl
    {
        private IContainer components = null;
        private byte[][] originalBoard, board;
        private byte[][][] candidates;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            base.AutoScaleMode = AutoScaleMode.Font;
            this.DoubleBuffered = true;
            base.Name = "SudokuBoard";
            base.Size = new Size(400, 400);
            base.Paint += new PaintEventHandler(SudokuBoard_Paint);
            base.Resize += new EventHandler(SudokuBoard_Resize);
            base.ResumeLayout(false);
        }

        public SudokuBoard()
        {
            InitializeComponent();
        }

        private void SudokuBoard_Paint(object sender, PaintEventArgs e)
        {
            Font f = base.Font, fMini = new Font(f.FontFamily, f.Size / 1.75f);
            e.Graphics.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);

            float w = (Width / 3f), h = (Height / 3f);
            for (byte i = 0; i < 3; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    e.Graphics.DrawRectangle(Pens.Black, (w * i) + 1, (h * j) + 1, w - 2, h - 2);
                }
            }

            w = (Width / 9f); h = (Height / 9f);
            for (byte i = 0; i < 9; i++)
            {
                for (byte j = 0; j < 9; j++)
                {
                    e.Graphics.DrawRectangle(Pens.Black, w * i, h * j, w, h);
                    if (board == null || candidates == null) continue;
                    if (board[i][j] != 0)
                        e.Graphics.DrawString(board[i][j].ToString(), f, board[i][j] == originalBoard[i][j] ? Brushes.Black : Brushes.DeepSkyBlue, w * i, h * j);
                    else
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            if (candidates[i][j][k] != 0)
                                e.Graphics.DrawString(candidates[i][j][k].ToString(), fMini, Brushes.Crimson, (w * i) + ((k % 3) * (w / 3)), (h * j) + ((k / 3) * (h / 3)));
                        }
                    }
                }
            }
        }

        private void SudokuBoard_Resize(object sender, EventArgs e) => Invalidate();

        public void SetBoard(byte[][] newBoard, byte[][][] possibilities)
        {
            if (newBoard.Length != 9 || newBoard[0].Length != 9) return;
            board = newBoard;
            originalBoard = Core.Utils.CopyBoard(board);
            candidates = possibilities;
            Invalidate();
        }
    }
}
