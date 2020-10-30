using Kermalis.SudokuSolver.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kermalis.SudokuSolver.UI
{
    internal sealed class SudokuBoard : UserControl
    {
        private const int SpaceBeforeGrid = 20;

        private readonly IContainer _components = null;
        private readonly Brush _changedText = Brushes.DodgerBlue,
            _candidateText = Brushes.Crimson,
            _culpritChangedHighlight = Brushes.Plum,
            _culpritHighlight = Brushes.Pink,
            _semiCulpritChangedHighlight = Brushes.CornflowerBlue,
            _semiCulpritHighlight = Brushes.Aquamarine,
            _changedHighlight = Brushes.Cornsilk;

        public delegate void CellChangedEventHandler(Cell cell);
        public event CellChangedEventHandler CellChanged;

        private Cell _selectedCell = null;
        private Puzzle _puzzle;
        private bool _showCandidates = false;
        private int _snapshotIndex = -1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && _components != null)
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;
            Name = "SudokuBoard";
            Size = new Size(450 + SpaceBeforeGrid, 450 + SpaceBeforeGrid);
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

        private void SudokuBoard_Paint(object sender, PaintEventArgs e)
        {
            Font f = base.Font, fMini = new Font(f.FontFamily, f.Size / 1.75f);
            float rWidth = Width - SpaceBeforeGrid, rHeight = Height - SpaceBeforeGrid;

            e.Graphics.DrawRectangle(Pens.Black, SpaceBeforeGrid, SpaceBeforeGrid, rWidth - 1, rHeight - 1);

            float w = rWidth / 3f, h = rHeight / 3f;
            bool b = true;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    var rect = new Rectangle((int)(w * x + SpaceBeforeGrid + 1), (int)(h * y + SpaceBeforeGrid + 1), (int)(w - 2), (int)(h - 2));
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
                    e.Graphics.DrawRectangle(Pens.Black, xoff + SpaceBeforeGrid, yoff + SpaceBeforeGrid, w, h);
                    if (_puzzle == null)
                    {
                        continue;
                    }

                    int val = _puzzle[x, y].Value;
                    IEnumerable<int> candidates = _puzzle[x, y].Candidates;

                    if (_snapshotIndex >= 0 && _snapshotIndex < _puzzle[x, y].Snapshots.Count)
                    {
                        CellSnapshot s = _puzzle[x, y].Snapshots[_snapshotIndex];
                        val = s.Value;
                        candidates = s.Candidates;
                        int xxoff = x % 3 == 0 ? 1 : 0, yyoff = y % 3 == 0 ? 1 : 0, // MATH
                            exoff = x % 3 == 2 ? 1 : 0, eyoff = y % 3 == 2 ? 1 : 0;
                        var rect = new RectangleF(xoff + SpaceBeforeGrid + 1 + xxoff, yoff + SpaceBeforeGrid + 1 + yyoff, w - 1 - xxoff - exoff, h - 1 - yyoff - eyoff);
                        bool changed = _snapshotIndex - 1 >= 0 && !new HashSet<int>(s.Candidates).SetEquals(_puzzle[x, y].Snapshots[_snapshotIndex - 1].Candidates);
                        Brush brush = null;
                        if (changed)
                        {
                            if (s.IsCulprit)
                            {
                                brush = _culpritChangedHighlight;
                            }
                            else if (s.IsSemiCulprit)
                            {
                                brush = _semiCulpritChangedHighlight;
                            }
                            else
                            {
                                brush = _changedHighlight;
                            }
                        }
                        else if (s.IsCulprit)
                        {
                            brush = _culpritHighlight;
                        }
                        else if (s.IsSemiCulprit)
                        {
                            brush = _semiCulpritHighlight;
                        }
                        if (brush != null)
                        {
                            e.Graphics.FillRectangle(brush, rect);
                        }
                    }

                    var point = new PointF(xoff + f.Size / 1.5f + SpaceBeforeGrid, yoff + f.Size / 2.25f + SpaceBeforeGrid);
                    if (_selectedCell != null && _selectedCell.Point.X == x && _selectedCell.Point.Y == y)
                    {
                        e.Graphics.DrawString("_", f, Brushes.Crimson, point);
                    }
                    if (val != 0)
                    {
                        e.Graphics.DrawString(val.ToString(), f, val == _puzzle[x, y].OriginalValue ? Brushes.Black : _changedText, point);
                    }
                    else if (_showCandidates)
                    {
                        foreach (int c in candidates)
                        {
                            e.Graphics.DrawString(c.ToString(), fMini, _candidateText, xoff + fMini.Size / 4 + (((c - 1) % 3) * (w / 3)) + SpaceBeforeGrid, yoff + (((c - 1) / 3) * (h / 3)) + SpaceBeforeGrid);
                        }
                    }
                }
            }
        }

        private Cell GetCellFromMouseLocation(Point location)
        {
            return (_puzzle == null || !_puzzle.IsCustom || location.X < SpaceBeforeGrid || location.Y < SpaceBeforeGrid) ? null : _puzzle[(location.X - SpaceBeforeGrid) / ((Width - SpaceBeforeGrid) / 9), (location.Y - SpaceBeforeGrid) / ((Height - SpaceBeforeGrid) / 9)];
        }

        private void SudokuBoard_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = GetCellFromMouseLocation(e.Location) == null ? Cursors.Default : Cursors.Hand;
        }

        private void SudokuBoard_Click(object sender, MouseEventArgs e)
        {
            _selectedCell = GetCellFromMouseLocation(e.Location);
            ReDraw(false);
        }

        private void SudokuBoard_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_selectedCell == null)
            {
                return;
            }

            if (e != null && ((e.KeyChar == '0' && _selectedCell.Value != 0) || (e.KeyChar > '0' && e.KeyChar <= '9')))
            {
                _selectedCell.ChangeOriginalValue(e.KeyChar - '0');
                _puzzle.LogAction(Puzzle.TechniqueFormat("Changed cell", _selectedCell.ToString()), _selectedCell, (Cell)null);
                CellChanged?.Invoke(_selectedCell);
            }
            _selectedCell = null;
            ReDraw(false);
        }

        private void SudokuBoard_Resize(object sender, EventArgs e)
        {
            ReDraw(_showCandidates);
        }
        public void ReDraw(bool showCandidates, int snapshot = -1)
        {
            _showCandidates = showCandidates;
            _snapshotIndex = snapshot;
            Invalidate();
        }

        public void SetBoard(Puzzle newBoard)
        {
            _puzzle = newBoard;
            ReDraw(false);
        }
    }
}
