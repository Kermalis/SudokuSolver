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
	/// <summary>The <see cref="NUM_VISIBLE_CELLS"/> cells this cell is grouped with. Block, Row, Column</summary>
	public ReadOnlyCollection<Cell> VisibleCells { get; private set; }

	public int Value { get; private set; }
	public Candidates Candidates { get; }

	public List<CellSnapshot> Snapshots { get; } // TODO: Let's have snapshots be part of the solver. Each action will capture the entire board instead.

	internal Cell(Puzzle puzzle, int value, SPoint point)
	{
		Puzzle = puzzle;

		OriginalValue = value;
		Value = value;
		Point = point;

		Candidates = new Candidates();
		Snapshots = [];

		// Will be set in InitRegions
		Block = null!;
		Column = null!;
		Row = null!;
		// Will be set in InitVisibleCells
		VisibleCells = null!;
	}
	internal void InitRegions()
	{
		Block = Puzzle.Blocks[Point.BlockIndex];
		Column = Puzzle.Columns[Point.Column];
		Row = Puzzle.Rows[Point.Row];
	}
	internal void InitVisibleCells()
	{
		int counter = 0;
		var neighbors = new Cell[NUM_VISIBLE_CELLS];
		for (int i = 0; i < 9; i++)
		{
			// Add 8 neighbors from block
			Cell other = Block[i];
			if (other != this)
			{
				neighbors[counter++] = other;
			}

			// Add 6 neighbors from row
			other = Row[i];
			if (other.Block != Block)
			{
				neighbors[counter++] = other;
			}

			// Add 6 neighbors from column
			other = Column[i];
			if (other.Block != Block)
			{
				neighbors[counter++] = other;
			}
		}
		VisibleCells = new ReadOnlyCollection<Cell>(neighbors);
	}

	// TODO: Remove
	internal bool ChangeCandidates(IEnumerable<int> candidates, bool remove = true)
	{
		bool changed = false;
		foreach (int candidate in candidates)
		{
			if (Candidates.Set(candidate, !remove))
			{
				changed = true;
			}
		}
		return changed;
	}
	internal static bool ChangeCandidates(IEnumerable<Cell> cells, int candidate, bool remove = true)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			if (cell.Candidates.Set(candidate, !remove))
			{
				changed = true;
			}
		}
		return changed;
	}
	internal static bool ChangeCandidates(IEnumerable<Cell> cells, IEnumerable<int> candidates, bool remove = true)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			foreach (int candidate in candidates)
			{
				if (cell.Candidates.Set(candidate, !remove))
				{
					changed = true;
				}
			}
		}
		return changed;
	}

	/// <summary>Changes the current value to <paramref name="newValue"/>. <see cref="Candidates"/> is updated.
	/// If <paramref name="refreshOtherCellCandidates"/> is true, the entire puzzle's candidates are refreshed.</summary>
	internal void Set(int newValue, bool refreshOtherCellCandidates = false)
	{
		int oldValue = Value;
		Value = newValue;

		if (newValue == EMPTY_VALUE)
		{
			for (int i = 1; i <= 9; i++)
			{
				Candidates.Set(i, true);
			}
			ChangeCandidates(VisibleCells, oldValue, remove: false);
		}
		else
		{
			Candidates.Clear(false);
			ChangeCandidates(VisibleCells, newValue);
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
		Set(value, refreshOtherCellCandidates: true);
	}
	internal void CreateSnapshot(bool isCulprit, bool isSemiCulprit)
	{
		Span<int> cache = stackalloc int[9]; // Don't just make an array of 9 since we may not use all of the slots
		Snapshots.Add(new CellSnapshot(Value, new ReadOnlyCollection<int>(Candidates.AsInt(cache).ToArray()), isCulprit, isSemiCulprit));
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
			s += "has candidates: " + Candidates.Print();
		}
		else
		{
			s += "- " + Value.ToString();
		}
		return s;
	}
#endif

	/*/// <summary>Returns the visible cells that this cell and <paramref name="other"/> share.
	/// Result length is 2 (1 row + 1 column) or 7 (7 row/column) or 13 (7 block + 6 row/column)</summary>
	internal Span<Cell> IntersectVisibleCells(Cell other, Span<Cell> cache)
	{
		int counter = 0;
		for (int i = 0; i < NUM_VISIBLE_CELLS; i++)
		{
			Cell cell = VisibleCells[i];
			if (other.VisibleCells.Contains(cell))
			{
				cache[counter++] = cell;
			}
		}
		return cache.Slice(0, counter);
	}
	/// <summary>Returns the visible cells that this cell, <paramref name="otherA"/>, and <paramref name="otherB"/> all share.
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
