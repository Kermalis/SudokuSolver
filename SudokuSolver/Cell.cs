using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if DEBUG
using System.Diagnostics;
#endif

namespace Kermalis.SudokuSolver;

#if DEBUG
[DebuggerDisplay("{DebugString()}", Name = "{ToString()}")]
#endif
public sealed class Cell
{
	public const int EMPTY_VALUE = 0;
	/// <summary>6 in Column, 6 in Row, 8 in Block</summary>
	public const int NUM_VISIBLE_CELLS = 6 + 6 + 8;

	internal readonly Puzzle Puzzle;
	public int OriginalValue { get; private set; }
	public SPoint Point { get; }
	public Region Block { get; private set; }
	public Region Column { get; private set; }
	public Region Row { get; private set; }
	internal readonly Cell[] VisibleI;

	public int Value { get; private set; }
	internal Candidates CandI;

	public Candidates Candidates => CandI;
	/// <summary>The <see cref="NUM_VISIBLE_CELLS"/> cells this cell is grouped with. Block, Row, Column</summary>
	public ReadOnlyCollection<Cell> VisibleCells { get; }

	internal Cell(Puzzle puzzle, int value, SPoint point)
	{
		Puzzle = puzzle;

		OriginalValue = value;
		Value = value;
		Point = point;

		CandI = new Candidates(true);
		VisibleI = new Cell[NUM_VISIBLE_CELLS]; // Will be init in InitVisibleCells
		VisibleCells = new ReadOnlyCollection<Cell>(VisibleI);

		// Will be set in InitRegions
		Block = null!;
		Column = null!;
		Row = null!;
	}
	internal void InitRegions()
	{
		Block = Puzzle.BlocksI[Point.BlockIndex];
		Column = Puzzle.ColumnsI[Point.Column];
		Row = Puzzle.RowsI[Point.Row];
	}
	internal void InitVisibleCells()
	{
		int counter = 0;
		for (int i = 0; i < 9; i++)
		{
			// Add 8 neighbors from block
			Cell other = Block[i];
			if (other != this)
			{
				VisibleI[counter++] = other;
			}

			// Add 6 neighbors from row
			other = Row[i];
			if (other.Block != Block)
			{
				VisibleI[counter++] = other;
			}

			// Add 6 neighbors from column
			other = Column[i];
			if (other.Block != Block)
			{
				VisibleI[counter++] = other;
			}
		}
	}

	// TODO: Remove
	internal bool ChangeCandidates(IEnumerable<int> digits, bool remove = true)
	{
		bool changed = false;
		foreach (int digit in digits)
		{
			if (CandI.Set(digit, !remove))
			{
				changed = true;
			}
		}
		return changed;
	}
	internal static bool ChangeCandidates(IEnumerable<Cell> cells, int digit, bool remove = true)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			if (cell.CandI.Set(digit, !remove))
			{
				changed = true;
			}
		}
		return changed;
	}
	internal static bool ChangeCandidates(IEnumerable<Cell> cells, IEnumerable<int> digits, bool remove = true)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			foreach (int digit in digits)
			{
				if (cell.CandI.Set(digit, !remove))
				{
					changed = true;
				}
			}
		}
		return changed;
	}

	/// <summary>Changes the current value to <paramref name="newValue"/>. <see cref="CandI"/> is updated.
	/// If <paramref name="refreshOtherCellCandidates"/> is true, the entire puzzle's candidates are refreshed.</summary>
	internal void SetValue(int newValue, bool refreshOtherCellCandidates = false)
	{
		int oldValue = Value;
		Value = newValue;

		if (newValue == EMPTY_VALUE)
		{
			for (int i = 1; i <= 9; i++)
			{
				CandI.Set(i, true);
			}
			Candidates.Set(VisibleI, oldValue, true);
		}
		else
		{
			CandI = new Candidates(false);
			Candidates.Set(VisibleI, newValue, false);
		}

		if (refreshOtherCellCandidates)
		{
			Puzzle.RefreshCandidates();
		}
	}
	public void ChangeOriginalValue(int value)
	{
		if (value is < EMPTY_VALUE or > 9)
		{
			throw new ArgumentOutOfRangeException(nameof(value), value, null);
		}

		OriginalValue = value;
		SetValue(value, refreshOtherCellCandidates: true);
	}

	public override int GetHashCode()
	{
		return Point.GetHashCode();
	}
	public override bool Equals(object? obj)
	{
		if (obj is Cell other)
		{
			return other.Point.Equals(Point);
		}
		return false;
	}
	public override string ToString()
	{
		return Point.ToString();
	}
#if DEBUG
	public string DebugString()
	{
		string s = Point.ToString() + " ";
		if (Value == EMPTY_VALUE)
		{
			s += "has candidates: " + CandI.Print();
		}
		else
		{
			s += "- " + Value.ToString();
		}
		return s;
	}
#endif

	/// <summary>Returns the visible cells that this cell and <paramref name="other"/> share.
	/// Result length is 2 (1 row + 1 column) or 7 (7 row/column) or 13 (7 block + 6 row/column)</summary>
	internal Span<Cell> IntersectVisibleCells(Cell other, Span<Cell> cache)
	{
		int counter = 0;
		for (int i = 0; i < VisibleI.Length; i++)
		{
			Cell cell = VisibleI[i];
			if (Array.IndexOf(other.VisibleI, cell) != -1)
			{
				cache[counter++] = cell;
			}
		}
		return cache.Slice(0, counter);
	}
	/*/// <summary>Returns the visible cells that this cell, <paramref name="otherA"/>, and <paramref name="otherB"/> all share.
	/// Result length is 0 or 1 (1 row/column) or 6 (6 block/row/column) or 12 (6 block + 6 row/column)</summary>
	internal Span<Cell> IntersectVisibleCells(Cell otherA, Cell otherB, Span<Cell> cache)
	{
		int counter = 0;
		for (int i = 0; i < NUM_VISIBLE_CELLS; i++)
		{
			Cell cell = VisibleCells[i];
			if (otherA.VisibleCells.Contains(cell) && otherB.VisibleCells.Contains(cell))
			{
				cache[counter++] = cell;
			}
		}
		return cache.Slice(0, counter);
	}*/

	/*/// <summary>Result length is 12 or 14</summary>
	internal Span<Cell> VisibleCellsExceptRegion(Region except, Span<Cell> cache)
	{
		int counter = 0;
		for (int i = 0; i < 8 + 6 + 6; i++)
		{
			Cell cell = VisibleCells[i];
			if (except.IndexOf(cell) == -1)
			{
				// Add if "except" region does not contain the visible cell
				cache[counter++] = cell;
			}
		}
		return cache.Slice(0, counter);
	}*/
}
