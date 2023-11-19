using System;
using System.Collections;
using System.Collections.Generic;

namespace Kermalis.SudokuSolver;

public sealed class Candidates : IEnumerable<int>
{
	private readonly bool[] _isCandidate;
	public int Count { get; private set; }

	/// <summary>true if the candidate is still possible</summary>
	public bool this[int can]
	{
		get => _isCandidate[can - 1];
		private set => _isCandidate[can - 1] = value;
	}

	public Candidates()
	{
		_isCandidate = new bool[9];
		Clear(true);
	}

	internal void Clear(bool possible)
	{
		for (int i = 0; i < 9; i++)
		{
			_isCandidate[i] = possible;
		}
		Count = possible ? 9 : 0;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal bool Set(int candidate, bool newState)
	{
		bool changed = this[candidate] != newState;
		if (changed)
		{
			if (newState)
			{
				Count++;
			}
			else
			{
				Count--;
			}
		}
		this[candidate] = newState;
		return changed;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal bool Set(ReadOnlySpan<int> candidates, bool newState)
	{
		bool changed = false;
		foreach (int can in candidates)
		{
			if (Set(can, newState))
			{
				changed = true;
			}
		}
		return changed;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal bool Set(IEnumerable<int> candidates, bool newState)
	{
		bool changed = false;
		foreach (int can in candidates)
		{
			if (Set(can, newState))
			{
				changed = true;
			}
		}
		return changed;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal static bool Set(ReadOnlySpan<Cell> cells, int candidate, bool newState)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			if (cell.Candidates.Set(candidate, newState))
			{
				changed = true;
			}
		}
		return changed;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal static bool Set(ReadOnlySpan<Cell> cells, ReadOnlySpan<int> candidates, bool newState)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			foreach (int cand in candidates)
			{
				if (cell.Candidates.Set(cand, newState))
				{
					changed = true;
				}
			}
		}
		return changed;
	}

	/// <summary>Result length is [0,9]</summary>
	internal Span<int> Intersect(Candidates other, Span<int> cache)
	{
		int retLength = 0;
		for (int can = 1; can <= 9; can++)
		{
			if (this[can] && other[can])
			{
				cache[retLength++] = can;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Result length is [0,9]</summary>
	internal Span<int> Intersect(Candidates otherA, Candidates otherB, Span<int> cache)
	{
		int retLength = 0;
		for (int can = 1; can <= 9; can++)
		{
			if (this[can] && otherA[can] && otherB[can])
			{
				cache[retLength++] = can;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Result length is [0,9]</summary>
	internal Span<int> Intersect(ReadOnlySpan<int> other, Span<int> cache)
	{
		int retLength = 0;
		for (int can = 1; can <= 9; can++)
		{
			if (this[can] && other.SimpleIndexOf(can) != -1)
			{
				cache[retLength++] = can;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Returns all candidates except for the ones in <paramref name="other"/>.
	/// Result length is [0,8]</summary>
	internal Span<int> Except(Candidates other, Span<int> cache)
	{
		int retLength = 0;
		for (int can = 1; can <= 9; can++)
		{
			if (this[can] && !other[can])
			{
				cache[retLength++] = can;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Returns all candidates except for the ones in <paramref name="other"/>.
	/// Result length is [0,8]</summary>
	internal Span<int> Except(ReadOnlySpan<int> other, Span<int> cache)
	{
		int retLength = 0;
		for (int can = 1; can <= 9; can++)
		{
			if (this[can] && other.SimpleIndexOf(can) == -1)
			{
				cache[retLength++] = can;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Returns all candidates except for <paramref name="other"/>.
	/// Result length is [0,8]</summary>
	internal Span<int> Except(int other, Span<int> cache)
	{
		int retLength = 0;
		for (int can = 1; can <= 9; can++)
		{
			if (can != other && this[can])
			{
				cache[retLength++] = can;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Result length is [0,9]</summary>
	internal static Span<int> Union(ReadOnlySpan<Cell> cells, Span<int> cache)
	{
		int retLength = 0;
		for (int can = 1; can <= 9; can++)
		{
			foreach (Cell cell in cells)
			{
				if (cell.Candidates[can])
				{
					cache[retLength++] = can;
					break;
				}
			}
		}
		return cache.Slice(0, retLength);
	}

	/// <summary>Returns these candidates as ints in the range [1,9].
	/// Result length is [0,9]</summary>
	internal Span<int> AsInt(Span<int> cache)
	{
		int retLength = 0;
		for (int can = 1; can <= 9; can++)
		{
			if (this[can])
			{
				cache[retLength++] = can;
			}
		}
		return cache.Slice(0, retLength);
	}

	/// <summary>Returns the cells from <paramref name="cells"/> that have <paramref name="can"/>.
	/// Result length is [0,cells.Length)</summary>
	internal static Span<Cell> WithCandidate(ReadOnlySpan<Cell> cells, int can, Span<Cell> cache)
	{
		int retLength = 0;
		for (int i = 0; i < cells.Length; i++)
		{
			Cell cell = cells[i];
			if (cell.Candidates[can])
			{
				cache[retLength++] = cell;
			}
		}
		return cache.Slice(0, retLength);
	}

	public string Print()
	{
		Span<int> span = stackalloc int[9];
		return "( " + string.Join(", ", AsInt(span).ToArray()) + " )";
	}
	/// <summary>Returns true if <paramref name="other"/> has the same candidates as <c>this</c></summary>
	public bool SetEquals(Candidates other)
	{
		for (int can = 1; can <= 9; can++)
		{
			if (this[can] != other[can])
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns true if <see cref="Count"/> is 1, and sets <paramref name="can1"/> to the candidate.</summary>
	internal bool TryGetCount1(out int can1)
	{
		if (Count != 1)
		{
			can1 = -1;
			return false;
		}

		for (int can = 1; can <= 9; can++)
		{
			if (this[can])
			{
				can1 = can;
				return true;
			}
		}
		throw new InvalidOperationException();
	}
	/// <summary>Returns true if <see cref="Count"/> is 2, and sets <paramref name="can1"/> and <paramref name="can2"/> to the candidates.</summary>
	internal bool TryGetCount2(out int can1, out int can2)
	{
		can1 = -1;
		can2 = -1;

		if (Count != 2)
		{
			return false;
		}

		int counter = 0;
		for (int can = 1; can <= 9; can++)
		{
			if (!this[can])
			{
				continue;
			}

			if (counter++ == 0)
			{
				can1 = can;
			}
			else
			{
				can2 = can;
				break;
			}
		}
		return true;
	}

	// TODO: Eventually remove..?
	public IEnumerator<int> GetEnumerator()
	{
		for (int i = 0; i < 9; i++)
		{
			if (_isCandidate[i])
			{
				yield return i + 1;
			}
		}
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
