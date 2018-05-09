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
        IContainer components = null;
        readonly int d = 20;
        readonly Brush changedText = Brushes.DodgerBlue, candidateText = Brushes.Crimson,
            culpritChangedHighlight = Brushes.Plum, culpritHighlight = Brushes.PaleTurquoise, changedHighlight = Brushes.PeachPuff;

        public delegate void CellChangedEventHandler(Cell cell);
        public event CellChangedEventHandler CellChanged;
        SPoint selected = null;

        Board board;

        bool bCandidates = false;
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
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;
            Name = "SudokuBoard";
            Size = new Size(450 + d, 450 + d);
            Paint += SudokuBoard_Paint;
            MouseMove += SudokuBoard_MouseMove;
            MouseClick += SudokuBoard_Click;
            KeyPress += SudokuBoard_KeyPress;
            LostFocus += (sender, e) => SudokuBoard_KeyPress(sender, null);
            Resize += SudokuBoard_Resize;
            ResumeLayout(false);
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

                    if (snap >= 0 && snap < board[x, y].Snapshots.Length)
                    {
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

                    var point = new PointF(xoff + f.Size / 1.5f + d, yoff + f.Size / 2.25f + d);
                    if (selected != null && selected.X == x && selected.Y == y)
                        e.Graphics.DrawString("_", f, Brushes.Crimson, point);
                    if (val != 0)
                        e.Graphics.DrawString(val.ToString(), f, val == board[x, y].OriginalValue ? Brushes.Black : changedText, point);
                    else if (bCandidates)
                        foreach (int v in cand)
                            e.Graphics.DrawString(v.ToString(), fMini, candidateText, xoff + fMini.Size / 4 + (((v - 1) % 3) * (w / 3)) + d, yoff + (((v - 1) / 3) * (h / 3)) + d);
                }
            }
        }

        private SPoint GetCellFromPosition(Point location) => (board == null || !board.IsCustom || location.X < d || location.Y < d) ? null : new SPoint((location.X - d) / ((Width - d) / 9), (location.Y - d) / ((Height - d) / 9));

        private void SudokuBoard_MouseMove(object sender, MouseEventArgs e) => Cursor = GetCellFromPosition(e.Location) == null ? Cursors.Default : Cursors.Hand;
        private void SudokuBoard_Click(object sender, MouseEventArgs e)
        {
            selected = GetCellFromPosition(e.Location);
            ReDraw(false);
        }
        private void SudokuBoard_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (selected == null) return;

            if (e != null && ((e.KeyChar == 48 && board[selected] != 0) || (e.KeyChar > 48 && e.KeyChar <= 57)))
            {
                board[selected].ChangeOriginal(e.KeyChar - 48);
                board.Log("Changed cell", new Cell[] { board[selected] }, board[selected].ToString());
                CellChanged?.Invoke(board[selected]);
            }
            selected = null;
            ReDraw(false);
        }

        void SudokuBoard_Resize(object sender, EventArgs e) => ReDraw(bCandidates);
        public void ReDraw(bool showCandidates, int snapshot = -1)
        {
            bCandidates = showCandidates;
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
