using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kermalis.SudokuSolver;

public sealed class Region : IEnumerable<Cell>
{
	private readonly Cell[] _cells;

	public Cell this[int index] => _cells[index];

	internal Region(ReadOnlySpan<Cell> cells)
	{
		_cells = cells.ToArray();
	}

	public IEnumerator<Cell> GetEnumerator()
	{
		return ((IEnumerable<Cell>)_cells).GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _cells.GetEnumerator();
	}

	public int IndexOf(Cell cell)
	{
		for (int i = 0; i < 9; i++)
		{
			if (_cells[i] == cell)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Result length is [0,9]</summary>
	internal Span<Cell> GetCellsWithCandidate(int digit, Span<Cell> cache)
	{
		return Candidates.GetCellsWithCandidate(_cells, digit, cache);
	}
	public IEnumerable<Cell> GetCellsWithCandidate(int candidate)
	{
		return _cells.Where(c => c.CandI.Contains(candidate));
	}

	/// <summary>Result length is [0,9]</summary>
	internal Span<Cell> GetCellsWithCandidateCount(int numCandidates, Span<Cell> cache)
	{
		int counter = 0;
		for (int i = 0; i < 9; i++)
		{
			Cell cell = _cells[i];
			if (cell.Candidates.Count == numCandidates)
			{
				cache[counter++] = cell;
			}
		}
		return cache.Slice(0, counter);
	}
	internal int CountCellsWithCandidates()
	{
		int counter = 0;
		for (int i = 0; i < 9; i++)
		{
			Cell cell = _cells[i];
			if (cell.CandI.Count != 0)
			{
				counter++;
			}
		}
		return counter;
	}

	/// <summary>Returns all cells except for the ones in <paramref name="other"/>.
	/// Result length is [0,9]</summary>
	internal Span<Cell> Except(ReadOnlySpan<Cell> other, Span<Cell> cache)
	{
		int retLength = 0;
		for (int i = 0; i < 9; i++)
		{
			Cell c = _cells[i];
			if (other.SimpleIndexOf(c) == -1)
			{
				cache[retLength++] = c;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Returns all cells except for the ones in <paramref name="other"/>.
	/// Result length is [0,9]</summary>
	internal Span<Cell> Except(Region other, Span<Cell> cache)
	{
		return Except(other._cells, cache);
	}

	/// <summary>Returns true if this region has more than one cell with the value of <paramref name="digit"/>.</summary>
	internal bool CheckForDuplicateValue(int digit)
	{
		bool foundValueAlready = false;
		for (int i = 0; i < 9; i++)
		{
			if (_cells[i].Value == digit)
			{
				if (foundValueAlready)
				{
					return true;
				}
				foundValueAlready = true;
			}
		}
		return false;
	}
}