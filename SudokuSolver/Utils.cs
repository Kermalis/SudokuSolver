using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kermalis.SudokuSolver;

internal static class Utils
{
	public static ReadOnlyCollection<int> OneToNine { get; } = new ReadOnlyCollection<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
	public static ReadOnlySpan<int> OneToNineSpan => new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

	/// <summary>outCol must be 3 length</summary>
	public static void GetColumnInBlock(this Cell[] block, int x, Span<Cell> outCol)
	{
		for (int col = 0; col < 3; col++)
		{
			outCol[col] = block[(x * 3) + col];
		}
	}
	/// <summary>outCol must be 3 length</summary>
	public static void GetRowInBlock(this Cell[] block, int y, Span<Cell> outRow)
	{
		for (int row = 0; row < 3; row++)
		{
			outRow[row] = block[(row * 3) + y];
		}
	}
	public static Cell[] GetColumnInBlock(this Cell[] block, int x)
	{
		var column = new Cell[3];
		for (int i = 0; i < 3; i++)
		{
			column[i] = block[(x * 3) + i];
		}
		return column;
	}
	public static Cell[] GetRowInBlock(this Cell[] block, int y)
	{
		var row = new Cell[3];
		for (int i = 0; i < 3; i++)
		{
			row[i] = block[(i * 3) + y];
		}
		return row;
	}

	public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> values)
	{
		foreach (T o in values)
		{
			if (source.Contains(o))
			{
				return true;
			}
		}
		return false;
	}
	public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
	{
		foreach (T o in values)
		{
			if (!source.Contains(o))
			{
				return false;
			}
		}
		return true;
	}
	public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> source)
	{
		if (!source.Any())
		{
			return Array.Empty<T>();
		}

		IEnumerable<T>[] inp = source.ToArray();
		IEnumerable<T> output = inp[0];
		for (int i = 1; i < inp.Length; i++)
		{
			output = output.Intersect(inp[i]);
		}
		return output;
	}
	public static IEnumerable<T> UniteAll<T>(this IEnumerable<IEnumerable<T>> source)
	{
		IEnumerable<T> output = Array.Empty<T>();
		foreach (IEnumerable<T> i in source)
		{
			output = output.Union(i);
		}
		return output;
	}

	public static string SingleOrMultiToString<T>(this IEnumerable<T> source)
	{
		int i = 0;
		foreach (T o in source)
		{
			if (++i > 1)
			{
				return source.Print();
			}
		}
		if (i == 0)
		{
			throw new ArgumentOutOfRangeException(nameof(source), "No elements in source");
		}
		return source.ElementAt(0)!.ToString()!;
	}
	public static string Print<T>(this IEnumerable<T> source)
	{
		return "( " + string.Join(", ", source) + " )";
	}
	public static string Print<T>(this T[] source)
	{
		return "( " + string.Join(", ", source) + " )"; // TODO: Deal with span allocs here. Have PrintCandidates and PrintCells
	}
	/*public static string PrintCells(ReadOnlySpan<Cell> cells)
	{

	}
	public static string PrintCandidates(ReadOnlySpan<int> candidates)
	{

	}*/

	public static int SimpleIndexOf(this ReadOnlySpan<Cell> cells, Cell cell)
	{
		for (int i = 0; i < cells.Length; i++)
		{
			if (cells[i].Point == cell.Point)
			{
				return i;
			}
		}
		return -1;
	}
	public static int SimpleIndexOf(this ReadOnlySpan<int> candidates, int can)
	{
		for (int i = 0; i < candidates.Length; i++)
		{
			if (candidates[i] == can)
			{
				return i;
			}
		}
		return -1;
	}
}
