using SudokuSolver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuSolver.UI
{
    public class SudokuBoard : UserControl
    {
        IContainer components;
        readonly int d = 20;

        readonly Brush changedText = Brushes.DodgerBlue, candidateText = Brushes.Crimson,
            culpritChangedHighlight = Brushes.Plum, culpritHighlight = Brushes.PaleTurquoise, changedHighlight = Brushes.PeachPuff;

        Board board;

        bool candidates = false;
        int snap = -1;

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
            bool b = true;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    var rect = new Rectangle((int)(w * x + d + 1), (int)(h * y + d + 1), (int)(w - 2), (int)(h - 2));
                    e.Graphics.FillRectangle((b = !b) ? Brushes.AliceBlue : Brushes.GhostWhite, rect);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                }
            }

            w = (rWidth / 9f); h = (rHeight / 9f);
            for (int x = 0; x < 9; x++)
            {
                float xoff = w * x;
                e.Graphics.DrawString((x + 1).ToString(), fMini, Brushes.Black, xoff + w / 1.3f, 0);
                e.Graphics.DrawString(SPoint.RowL(x), fMini, Brushes.Black, 0, h * x + h / 1.4f);
                for (int y = 0; y < 9; y++)
                {
                    float yoff = h * y;
                    e.Graphics.DrawRectangle(Pens.Black, xoff + d, yoff + d, w, h);
                    if (board == null) continue;

                    int val = board[x, y];
                    IEnumerable<int> cand = board[x, y].Candidates;

                    if (snap >= 0 && snap < board[x, y].Snapshots.Length) {
                        Snapshot s = board[x, y].Snapshots[snap];
                        val = s.Value;
                        cand = s.Candidates;
                        int xxoff = x % 3 == 0 ? 1 : 0, yyoff = y % 3 == 0 ? 1 : 0, // MATH
                            exoff = x % 3 == 2 ? 1 : 0, eyoff = y % 3 == 2 ? 1 : 0;
                        var rect = new RectangleF(xoff + d + 1 + xxoff, yoff + d + 1 + yyoff, w - 1 - xxoff - exoff, h - 1 - yyoff - eyoff);
                        bool changed = snap - 1 >= 0 && s.Candidates.Length != board[x, y].Snapshots[snap - 1].Candidates.Length;
                        if (s.IsCulprit && changed)
                            e.Graphics.FillRectangle(culpritChangedHighlight, rect);
                        else if (s.IsCulprit)
                            e.Graphics.FillRectangle(culpritHighlight, rect);
                        else if (changed)
                            e.Graphics.FillRectangle(changedHighlight, rect);
                    }

                    if (val != 0)
                        e.Graphics.DrawString(val.ToString(), f, val == board[x, y].OriginalValue ? Brushes.Black : changedText, xoff + f.Size / 1.5f + d, yoff + f.Size / 2.25f + d);
                    else if (candidates)
                        foreach (int v in cand)
                            e.Graphics.DrawString(v.ToString(), fMini, candidateText, xoff + fMini.Size / 4 + (((v - 1) % 3) * (w / 3)) + d, yoff + (((v - 1) / 3) * (h / 3)) + d);
                }
            }
        }

        void SudokuBoard_Resize(object sender, EventArgs e) => ReDraw(candidates);
        public void ReDraw(bool showCandidates, int snapshot = -1)
        {
            candidates = showCandidates;
            snap = snapshot;
            Invalidate();
        }

        public void SetBoard(Board newBoard)
        {
            board = newBoard;
            ReDraw(false);
        }
    }
}
