using Kermalis.SudokuSolver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kermalis.SudokuSolver.UI
{
    class SudokuBoard : UserControl
    {
        IContainer components = null;
        const int spaceBeforeGrid = 20;
        readonly Brush changedText = Brushes.DodgerBlue, candidateText = Brushes.Crimson,
            culpritChangedHighlight = Brushes.Plum, culpritHighlight = Brushes.PaleTurquoise, changedHighlight = Brushes.PeachPuff;

        public delegate void CellChangedEventHandler(Cell cell);
        public event CellChangedEventHandler CellChanged;
        Cell selectedCell = null;

        Puzzle puzzle;

        bool showCandidates = false;
        int snapshotIndex = -1;

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
            Size = new Size(450 + spaceBeforeGrid, 450 + spaceBeforeGrid);
            Paint += SudokuBoard_Paint;
            MouseMove += SudokuBoard_MouseMove;
            MouseClick += SudokuBoard_Click;
            KeyPress += SudokuBoard_KeyPress;
            LostFocus += (sender, e) => SudokuBoard_KeyPress(sender, null);
            Resize += SudokuBoard_Resize;
            ResumeLayout(false);
        }
        public SudokuBoard()
        {
            InitializeComponent();
        }

        void SudokuBoard_Paint(object sender, PaintEventArgs e)
        {
            Font f = base.Font, fMini = new Font(f.FontFamily, f.Size / 1.75f);
            float rWidth = Width - spaceBeforeGrid, rHeight = Height - spaceBeforeGrid;

            e.Graphics.DrawRectangle(Pens.Black, spaceBeforeGrid, spaceBeforeGrid, rWidth - 1, rHeight - 1);

            float w = rWidth / 3f, h = rHeight / 3f;
            bool b = true;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    var rect = new Rectangle((int)(w * x + spaceBeforeGrid + 1), (int)(h * y + spaceBeforeGrid + 1), (int)(w - 2), (int)(h - 2));
                    e.Graphics.FillRectangle((b = !b) ? Brushes.AliceBlue : Brushes.GhostWhite, rect);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                }
            }

            w = rWidth / 9f;
            h = rHeight / 9f;
            for (int x = 0; x < 9; x++)
            {
                float xoff = w * x;
                e.Graphics.DrawString(SPoint.ColumnLetter(x), fMini, Brushes.Black, xoff + w / 1.3f, 0);
                e.Graphics.DrawString(SPoint.RowLetter(x), fMini, Brushes.Black, 0, h * x + h / 1.4f);
                for (int y = 0; y < 9; y++)
                {
                    float yoff = h * y;
                    e.Graphics.DrawRectangle(Pens.Black, xoff + spaceBeforeGrid, yoff + spaceBeforeGrid, w, h);
                    if (puzzle == null)
                    {
                        continue;
                    }

                    int val = puzzle[x, y].Value;
                    IEnumerable<int> candidates = puzzle[x, y].Candidates;

                    if (snapshotIndex >= 0 && snapshotIndex < puzzle[x, y].Snapshots.Count)
                    {
                        CellSnapshot s = puzzle[x, y].Snapshots[snapshotIndex];
                        val = s.Value;
                        candidates = s.Candidates;
                        int xxoff = x % 3 == 0 ? 1 : 0, yyoff = y % 3 == 0 ? 1 : 0, // MATH
                            exoff = x % 3 == 2 ? 1 : 0, eyoff = y % 3 == 2 ? 1 : 0;
                        var rect = new RectangleF(xoff + spaceBeforeGrid + 1 + xxoff, yoff + spaceBeforeGrid + 1 + yyoff, w - 1 - xxoff - exoff, h - 1 - yyoff - eyoff);
                        bool changed = snapshotIndex - 1 >= 0 && !new HashSet<int>(s.Candidates).SetEquals(puzzle[x, y].Snapshots[snapshotIndex - 1].Candidates);
                        if (s.IsCulprit && changed)
                        {
                            e.Graphics.FillRectangle(culpritChangedHighlight, rect);
                        }
                        else if (s.IsCulprit)
                        {
                            e.Graphics.FillRectangle(culpritHighlight, rect);
                        }
                        else if (changed)
                        {
                            e.Graphics.FillRectangle(changedHighlight, rect);
                        }
                    }

                    var point = new PointF(xoff + f.Size / 1.5f + spaceBeforeGrid, yoff + f.Size / 2.25f + spaceBeforeGrid);
                    if (selectedCell != null && selectedCell.Point.X == x && selectedCell.Point.Y == y)
                    {
                        e.Graphics.DrawString("_", f, Brushes.Crimson, point);
                    }
                    if (val != 0)
                    {
                        e.Graphics.DrawString(val.ToString(), f, val == puzzle[x, y].OriginalValue ? Brushes.Black : changedText, point);
                    }
                    else if (showCandidates)
                    {
                        foreach (int c in candidates)
                        {
                            e.Graphics.DrawString(c.ToString(), fMini, candidateText, xoff + fMini.Size / 4 + (((c - 1) % 3) * (w / 3)) + spaceBeforeGrid, yoff + (((c - 1) / 3) * (h / 3)) + spaceBeforeGrid);
                        }
                    }
                }
            }
        }

        Cell GetCellFromMouseLocation(Point location)
        {
            return (puzzle == null || !puzzle.IsCustom || location.X < spaceBeforeGrid || location.Y < spaceBeforeGrid) ? null : puzzle[(location.X - spaceBeforeGrid) / ((Width - spaceBeforeGrid) / 9), (location.Y - spaceBeforeGrid) / ((Height - spaceBeforeGrid) / 9)];
        }

        void SudokuBoard_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = GetCellFromMouseLocation(e.Location) == null ? Cursors.Default : Cursors.Hand;
        }
        void SudokuBoard_Click(object sender, MouseEventArgs e)
        {
            selectedCell = GetCellFromMouseLocation(e.Location);
            ReDraw(false);
        }
        void SudokuBoard_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (selectedCell == null)
            {
                return;
            }

            if (e != null && ((e.KeyChar == 48 && selectedCell.Value != 0) || (e.KeyChar > 48 && e.KeyChar <= 57)))
            {
                selectedCell.ChangeOriginalValue(e.KeyChar - 48);
                puzzle.LogAction("Changed cell", new Cell[] { selectedCell }, selectedCell.ToString());
                CellChanged?.Invoke(selectedCell);
            }
            selectedCell = null;
            ReDraw(false);
        }

        void SudokuBoard_Resize(object sender, EventArgs e)
        {
            ReDraw(showCandidates);
        }
        public void ReDraw(bool showCandidates, int snapshot = -1)
        {
            this.showCandidates = showCandidates;
            snapshotIndex = snapshot;
            Invalidate();
        }

        public void SetBoard(Puzzle newBoard)
        {
            puzzle = newBoard;
            ReDraw(false);
        }
    }
}
