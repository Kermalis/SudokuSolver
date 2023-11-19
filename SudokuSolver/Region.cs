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

	/*/// <summary>Result length is [0,9]</summary>
	internal Span<Cell> GetCellsWithCandidate(int candidate, Span<Cell> cache)
	{
		int counter = 0;
		for (int i = 0; i < 9; i++)
		{
			Cell cell = _cells[i];
			if (cell.Candidates[candidate])
			{
				cache[counter++] = cell;
			}
		}
		return cache.Slice(0, counter);
	}*/
	public IEnumerable<Cell> GetCellsWithCandidate(int candidate)
	{
		return _cells.Where(c => c.Candidates.Contains(candidate));
	}
	public IEnumerable<Cell> GetCellsWithCandidates(params int[] candidates)
	{
		return _cells.Where(c => c.Candidates.ContainsAll(candidates));
	}

	/*/// <summary>Result length is [0,9]</summary>
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
			if (cell.Candidates.Count != 0)
			{
				counter++;
			}
		}
		return counter;
	}*/

	/*/// <summary>Returns all cells except for the ones in <paramref name="other"/>.
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
	}*/

	internal bool CheckForDuplicateValue(int val)
	{
		bool foundValueAlready = false;
		for (int i = 0; i < 9; i++)
		{
			if (_cells[i].Value == val)
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
