using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Kermalis.SudokuSolver.Core;

[DebuggerDisplay("{DebugString()}", Name = "{ToString()}")]
internal sealed class Cell
{
	public const int EMPTY_VALUE = 0;

	private readonly Puzzle _puzzle;
	public int OriginalValue { get; private set; }
	public SPoint Point { get; }
	/// <summary>All other cells this cell is grouped with. Block, Row, Column</summary>
	public ReadOnlyCollection<Cell> VisibleCells { get; private set; }

	public int Value { get; private set; }
	public HashSet<int> Candidates { get; }

	public List<CellSnapshot> Snapshots { get; }

	public Cell(Puzzle puzzle, int value, SPoint point)
	{
		_puzzle = puzzle;

		OriginalValue = value;
		Value = value;
		Point = point;

		Candidates = new HashSet<int>(Utils.OneToNine);
		Snapshots = new List<CellSnapshot>();

		VisibleCells = null!; // Will be set in CalcVisibleCells
	}
	public void CalcVisibleCells()
	{
		Region block = _puzzle.Blocks[Point.BlockIndex];
		Region col = _puzzle.Columns[Point.X];
		Region row = _puzzle.Rows[Point.Y];
		VisibleCells = new ReadOnlyCollection<Cell>(block.Union(col).Union(row).Except(new Cell[] { this }).ToArray());
	}

	public bool ChangeCandidates(int candidate, bool remove = true)
	{
		return remove ? Candidates.Remove(candidate) : Candidates.Add(candidate);
	}
	public bool ChangeCandidates(IEnumerable<int> candidates, bool remove = true)
	{
		bool changed = false;
		foreach (int value in candidates)
		{
			if (remove ? Candidates.Remove(value) : Candidates.Add(value))
			{
				changed = true;
			}
		}
		return changed;
	}
	public static bool ChangeCandidates(IEnumerable<Cell> cells, int candidate, bool remove = true)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			if (remove ? cell.Candidates.Remove(candidate) : cell.Candidates.Add(candidate))
			{
				changed = true;
			}
		}
		return changed;
	}
	public static bool ChangeCandidates(IEnumerable<Cell> cells, IEnumerable<int> candidates, bool remove = true)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			foreach (int value in candidates)
			{
				if (remove ? cell.Candidates.Remove(value) : cell.Candidates.Add(value))
				{
					changed = true;
				}
			}
		}
		return changed;
	}

	public void Set(int newValue, bool refreshOtherCellCandidates = false)
	{
		int oldValue = Value;
		Value = newValue;

		if (newValue == EMPTY_VALUE)
		{
			for (int i = 1; i <= 9; i++)
			{
				Candidates.Add(i);
			}
			ChangeCandidates(VisibleCells, oldValue, remove: false);
		}
		else
		{
			Candidates.Clear();
			ChangeCandidates(VisibleCells, newValue);
		}

		if (refreshOtherCellCandidates)
		{
			_puzzle.RefreshCandidates();
		}
	}
	public void ChangeOriginalValue(int value)
	{
		OriginalValue = value;
		Set(value, refreshOtherCellCandidates: true);
	}
	public void CreateSnapshot(bool isCulprit, bool isSemiCulprit)
	{
		Snapshots.Add(new CellSnapshot(Value, Candidates, isCulprit, isSemiCulprit));
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
}
