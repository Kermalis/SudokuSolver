using System;
using System.Collections;
using System.Collections.Generic;

namespace Kermalis.SudokuSolver;

public struct Candidates : IEnumerable<int>
{
	// First 9 bits are flags for valid candidates. Next 4 bits are for the count [0, 9]. Last 3 bits unused
	private const ushort ALL_POSSIBLE = 0b1_1111_1111 | (9 << 9);
	private const ushort NONE_POSSIBLE = 0;

	private ushort _data;

	public readonly int Count => _data >> 9;

	internal Candidates(bool possible)
	{
		_data = possible ? ALL_POSSIBLE : NONE_POSSIBLE;
	}

	public readonly bool IsCandidate(int digit)
	{
		if (digit is < 1 or > 9)
		{
			throw new ArgumentOutOfRangeException(nameof(digit), digit, null);
		}

		return IsCandidate_Fast(digit - 1);
	}
	private readonly bool IsCandidate_Fast(int index)
	{
		int bit = 1 << index;
		return (_data & bit) != 0;
	}

	private void SetCount(int count)
	{
		_data &= 0b1110_0001_1111_1111;
		_data |= (ushort)(count << 9);
	}

	/// <summary>Returns true if there was a change.</summary>
	internal bool Set(int digit, bool newState)
	{
		int index = digit - 1;
		bool changed = IsCandidate_Fast(index) != newState;
		if (changed)
		{
			_data ^= (ushort)(1 << index);
			if (newState)
			{
				SetCount(Count + 1);
			}
			else
			{
				SetCount(Count - 1);
			}
		}

		return changed;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal bool Set(ReadOnlySpan<int> digit, bool newState)
	{
		bool changed = false;
		foreach (int can in digit)
		{
			if (Set(can, newState))
			{
				changed = true;
			}
		}
		return changed;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal bool Set(IEnumerable<int> digit, bool newState)
	{
		bool changed = false;
		foreach (int can in digit)
		{
			if (Set(can, newState))
			{
				changed = true;
			}
		}
		return changed;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal static bool Set(ReadOnlySpan<Cell> cells, int digit, bool newState)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			if (cell.CandI.Set(digit, newState))
			{
				changed = true;
			}
		}
		return changed;
	}
	/// <summary>Returns true if there was a change.</summary>
	internal static bool Set(ReadOnlySpan<Cell> cells, ReadOnlySpan<int> digit, bool newState)
	{
		bool changed = false;
		foreach (Cell cell in cells)
		{
			foreach (int cand in digit)
			{
				if (cell.CandI.Set(cand, newState))
				{
					changed = true;
				}
			}
		}
		return changed;
	}

	/// <summary>Result length is [0,9]</summary>
	internal readonly Span<int> Intersect(Candidates other, Span<int> cache)
	{
		int retLength = 0;
		for (int i = 0; i < 9; i++)
		{
			if (IsCandidate_Fast(i) && other.IsCandidate_Fast(i))
			{
				cache[retLength++] = i + 1;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Result length is [0,9]</summary>
	internal readonly Span<int> Intersect(Candidates otherA, Candidates otherB, Span<int> cache)
	{
		int retLength = 0;
		for (int i = 0; i < 9; i++)
		{
			if (IsCandidate_Fast(i) && otherA.IsCandidate_Fast(i) && otherB.IsCandidate_Fast(i))
			{
				cache[retLength++] = i + 1;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Result length is [0,9]</summary>
	internal readonly Span<int> Intersect(ReadOnlySpan<int> other, Span<int> cache)
	{
		int retLength = 0;
		for (int digit = 1; digit <= 9; digit++)
		{
			if (IsCandidate_Fast(digit - 1) && other.SimpleIndexOf(digit) != -1)
			{
				cache[retLength++] = digit;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Returns all candidates except for the ones in <paramref name="other"/>.
	/// Result length is [0,8]</summary>
	internal readonly Span<int> Except(Candidates other, Span<int> cache)
	{
		int retLength = 0;
		for (int i = 0; i < 9; i++)
		{
			if (IsCandidate_Fast(i) && !other.IsCandidate_Fast(i))
			{
				cache[retLength++] = i + 1;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Returns all candidates except for the ones in <paramref name="other"/>.
	/// Result length is [0,8]</summary>
	internal readonly Span<int> Except(ReadOnlySpan<int> other, Span<int> cache)
	{
		int retLength = 0;
		for (int digit = 1; digit <= 9; digit++)
		{
			if (IsCandidate_Fast(digit - 1) && other.SimpleIndexOf(digit) == -1)
			{
				cache[retLength++] = digit;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Returns all candidates except for <paramref name="other"/>.
	/// Result length is [0,8]</summary>
	internal readonly Span<int> Except(int other, Span<int> cache)
	{
		int retLength = 0;
		for (int digit = 1; digit <= 9; digit++)
		{
			if (digit != other && IsCandidate_Fast(digit - 1))
			{
				cache[retLength++] = digit;
			}
		}
		return cache.Slice(0, retLength);
	}
	/// <summary>Result length is [0,9]</summary>
	internal static Span<int> Union(ReadOnlySpan<Cell> cells, Span<int> cache)
	{
		int retLength = 0;
		for (int i = 0; i < 9; i++)
		{
			foreach (Cell cell in cells)
			{
				if (cell.CandI.IsCandidate_Fast(i))
				{
					cache[retLength++] = i + 1;
					break;
				}
			}
		}
		return cache.Slice(0, retLength);
	}

	/// <summary>Returns these candidates as ints in the range [1,9].
	/// Result length is [0,9]</summary>
	internal readonly Span<int> AsInt(Span<int> cache)
	{
		int retLength = 0;
		for (int i = 0; i < 9; i++)
		{
			if (IsCandidate_Fast(i))
			{
				cache[retLength++] = i + 1;
			}
		}
		return cache.Slice(0, retLength);
	}

	/// <summary>Returns the cells from <paramref name="cells"/> that have <paramref name="digit"/>.
	/// Result length is [0, cells.Length]</summary>
	internal static Span<Cell> GetCellsWithCandidate(ReadOnlySpan<Cell> cells, int digit, Span<Cell> cache)
	{
		int retLength = 0;
		for (int i = 0; i < cells.Length; i++)
		{
			Cell cell = cells[i];
			if (cell.CandI.IsCandidate_Fast(digit - 1))
			{
				cache[retLength++] = cell;
			}
		}
		return cache.Slice(0, retLength);
	}

	public readonly string Print()
	{
		Span<int> span = stackalloc int[9];
		return Utils.PrintCandidates(AsInt(span));
	}
	/// <summary>Returns true if <paramref name="other"/> has the same candidates as <c>this</c></summary>
	public readonly bool SetEquals(Candidates other)
	{
		return _data == other._data;
	}

	/// <summary>Returns true if <see cref="Count"/> is 1, and sets <paramref name="can1"/> to the candidate.</summary>
	internal readonly bool TryGetCount1(out int can1)
	{
		if (Count != 1)
		{
			can1 = -1;
			return false;
		}

		for (int i = 0; i < 9; i++)
		{
			if (IsCandidate_Fast(i))
			{
				can1 = i + 1;
				return true;
			}
		}
		throw new InvalidOperationException();
	}
	/// <summary>Returns true if <see cref="Count"/> is 2, and sets <paramref name="can1"/> and <paramref name="can2"/> to the candidates.</summary>
	internal readonly bool TryGetCount2(out int can1, out int can2)
	{
		can1 = -1;
		can2 = -1;

		if (Count != 2)
		{
			return false;
		}

		int counter = 0;
		for (int i = 0; i < 9; i++)
		{
			if (!IsCandidate_Fast(i))
			{
				continue;
			}

			if (counter++ == 0)
			{
				can1 = i + 1;
			}
			else
			{
				can2 = i + 1;
				break;
			}
		}
		return true;
	}
	/// <summary>Gets the first two candidates. Assumes <see cref="Count"/> is 2.</summary>
	internal readonly void GetCount2(out int can1, out int can2)
	{
		can1 = -1;
		can2 = -1;

		int counter = 0;
		for (int i = 0; i < 9; i++)
		{
			if (!IsCandidate_Fast(i))
			{
				continue;
			}

			if (counter++ == 0)
			{
				can1 = i + 1;
			}
			else
			{
				can2 = i + 1;
				break;
			}
		}
	}

	internal readonly bool HasBoth(int can1, int can2)
	{
		return IsCandidate_Fast(can1 - 1) && IsCandidate_Fast(can2 - 1);
	}

	// TODO: Eventually remove..?
	public readonly IEnumerator<int> GetEnumerator()
	{
		for (int i = 0; i < 9; i++)
		{
			if (IsCandidate_Fast(i))
			{
				yield return i + 1;
			}
		}
	}
	readonly IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}