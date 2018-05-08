using SudokuSolver.Core;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuSolver.UI
{
    public class SudokuBoard : UserControl
    {
        IContainer components;
        Board board;
        bool candidates = false;
        int d = 20;

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
            base.Size = new Size(450 + d, 450 + d);
            base.Paint += new PaintEventHandler(SudokuBoard_Paint);
            base.Resize += new EventHandler(SudokuBoard_Resize);
            base.ResumeLayout(false);
        }

        public SudokuBoard() => InitializeComponent();

        void SudokuBoard_Paint(object sender, PaintEventArgs e)
        {
            Font f = base.Font, fMini = new Font(f.FontFamily, f.Size / 1.75f);
            float rWidth = Width - d, rHeight = Height - d;

            e.Graphics.DrawRectangle(Pens.Black, d, d, rWidth - 1, rHeight - 1);

            float w = (rWidth / 3f), h = (rHeight / 3f);
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    e.Graphics.DrawRectangle(Pens.Black, w * x + d + 1, h * y + d + 1, w - 2, h - 2);
                }
            }

            w = (rWidth / 9f); h = (rHeight / 9f);
            for (int x = 0; x < 9; x++)
            {
                float xoff = w * x;
                e.Graphics.DrawString((x + 1).ToString(), fMini, Brushes.Black, xoff + w / 1.3f, 0);
                e.Graphics.DrawString(((char)(x + 65)).ToString(), fMini, Brushes.Black, 0, xoff + w / 1.4f);
                for (int y = 0; y < 9; y++)
                {
                    float yoff = h * y;
                    e.Graphics.DrawRectangle(Pens.Black, xoff + d, yoff + d, w, h);
                    if (board == null) continue;
                    if (board[x, y] != 0)
                        e.Graphics.DrawString(board[x, y].ToString(), f, board[x, y].Value == board[x, y].OriginalValue ? Brushes.Black : Brushes.DeepSkyBlue, xoff + f.Size / 1.5f + d, yoff + f.Size / 2.25f + d);
                    else if (candidates)
                        foreach (int v in board[x, y].Candidates)
                            e.Graphics.DrawString(v.ToString(), fMini, Brushes.Crimson, xoff + (((v - 1) % 3) * (w / 3)) + d, yoff + (((v - 1) / 3) * (h / 3)) + d);
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
