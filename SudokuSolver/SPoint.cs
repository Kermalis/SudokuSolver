using System;

namespace Kermalis.SudokuSolver;

public readonly struct SPoint
{
	public int Column { get; }
	public int Row { get; }
	public int BlockIndex { get; }

	internal SPoint(int col, int row)
	{
		Column = col;
		Row = row;
		BlockIndex = (col / 3) + (3 * (row / 3));
	}

	public bool Equals(int col, int row)
	{
		return Column == col && Row == row;
	}
	public static string RowLetter(int row)
	{
		return ((char)(row + 'A')).ToString();
	}
	public static string ColumnLetter(int col)
	{
		return (col + 1).ToString();
	}

	public static bool operator ==(SPoint left, SPoint right)
	{
		return left.Equals(right);
	}
	public static bool operator !=(SPoint left, SPoint right)
	{
		return !(left == right);
	}
	public override bool Equals(object? obj)
	{
		if (obj is SPoint other)
		{
			return other.Column == Column && other.Row == Row;
		}
		return false;
	}
	public override int GetHashCode()
	{
		return HashCode.Combine(Column, Row);
	}
	public override string ToString()
	{
		return RowLetter(Row) + ColumnLetter(Column);
	}
}