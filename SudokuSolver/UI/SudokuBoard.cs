using SudokuSolver.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuSolver.UI
{
    public class SudokuBoard : UserControl
    {
        IContainer components = null;
        Board board;
        bool candidates = false;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        void InitializeComponent()
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

        public SudokuBoard() => InitializeComponent();

        void SudokuBoard_Paint(object sender, PaintEventArgs e)
        {
            Font f = base.Font, fMini = new Font(f.FontFamily, f.Size / 1.75f);
            e.Graphics.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);

            float w = (Width / 3f), h = (Height / 3f);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    e.Graphics.DrawRectangle(Pens.Black, (w * i) + 1, (h * j) + 1, w - 2, h - 2);
                }
            }

            w = (Width / 9f); h = (Height / 9f);
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    e.Graphics.DrawRectangle(Pens.Black, w * x, h * y, w, h);
                    if (board == null) continue;
                    if (board[x, y] != 0)
                        e.Graphics.DrawString(board[x, y].ToString(), f, board[x, y].Value == board[x, y].OriginalValue ? Brushes.Black : Brushes.DeepSkyBlue, w * x, h * y);
                    else if (candidates)
                        foreach (int v in board[x, y].Candidates)
                            e.Graphics.DrawString(v.ToString(), fMini, Brushes.Crimson, (w * x) + (((v - 1) % 3) * (w / 3)), (h * y) + (((v - 1) / 3) * (h / 3)));
                }
            }
        }

        void SudokuBoard_Resize(object sender, EventArgs e) => ReDraw(candidates);
        public void ReDraw(bool showCandidates)
        {
            candidates = showCandidates;
            Invalidate();
        }

        public void SetBoard(Board newBoard)
        {
            board = newBoard;
            ReDraw(false);
        }
    }
}
