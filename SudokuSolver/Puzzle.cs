using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Kermalis.SudokuSolver;

public sealed class Puzzle
{
	public ReadOnlyCollection<Region> Rows { get; }
	public ReadOnlyCollection<Region> Columns { get; }
	public ReadOnlyCollection<Region> Blocks { get; }
	public ReadOnlyCollection<ReadOnlyCollection<Region>> Regions { get; }

	public bool IsCustom { get; }
	private readonly Cell[] _board;

	public Cell this[int col, int row] => _board[Utils.CellIndex(col, row)];

	private Puzzle(int[][] board, bool isCustom)
	{
		IsCustom = isCustom;

		_board = new Cell[81];
		for (int col = 0; col < 9; col++)
		{
			for (int row = 0; row < 9; row++)
			{
				_board[Utils.CellIndex(col, row)] = new Cell(this, board[col][row], new SPoint(col, row));
			}
		}

		var rows = new Region[9];
		var columns = new Region[9];
		var blocks = new Region[9];
		var cells = new Cell[9];
		for (int i = 0; i < 9; i++)
		{
			int j;
			for (j = 0; j < 9; j++)
			{
				cells[j] = _board[Utils.CellIndex(j, i)];
			}
			rows[i] = new Region(cells);

			for (j = 0; j < 9; j++)
			{
				cells[j] = _board[Utils.CellIndex(i, j)];
			}
			columns[i] = new Region(cells);

			j = 0;
			int x = i % 3 * 3;
			int y = i / 3 * 3;
			for (int col = x; col < x + 3; col++)
			{
				for (int row = y; row < y + 3; row++)
				{
					cells[j++] = _board[Utils.CellIndex(col, row)];
				}
			}
			blocks[i] = new Region(cells);
		}

		Regions = new ReadOnlyCollection<ReadOnlyCollection<Region>>(new ReadOnlyCollection<Region>[3]
		{
			Rows = new ReadOnlyCollection<Region>(rows),
			Columns = new ReadOnlyCollection<Region>(columns),
			Blocks = new ReadOnlyCollection<Region>(blocks)
		});

		for (int i = 0; i < 81; i++)
		{
			_board[i].InitRegions();
		}
		for (int i = 0; i < 81; i++)
		{
			_board[i].InitVisibleCells();
		}
	}

	internal void RefreshCandidates()
	{
		for (int i = 0; i < 81; i++)
		{
			Cell cell = _board[i];
			for (int digit = 1; digit <= 9; digit++)
			{
				cell.CandI.Set(digit, true);
			}
		}
		for (int i = 0; i < 81; i++)
		{
			Cell cell = _board[i];
			if (cell.Value != Cell.EMPTY_VALUE)
			{
				cell.Set(cell.Value);
			}
		}
	}

	public static Puzzle CreateCustom()
	{
		int[][] board = new int[9][];
		for (int col = 0; col < 9; col++)
		{
			board[col] = new int[9];
		}
		return new Puzzle(board, true);
	}
	public static Puzzle Parse(ReadOnlySpan<string> inRows)
	{
		if (inRows.Length != 9)
		{
			throw new InvalidDataException("Puzzle must have 9 rows.");
		}

		int[][] board = new int[9][];
		for (int col = 0; col < 9; col++)
		{
			board[col] = new int[9];
		}

		for (int row = 0; row < 9; row++)
		{
			string line = inRows[row];
			if (line.Length != 9)
			{
				throw new InvalidDataException($"Row {row} must have 9 values.");
			}

			for (int col = 0; col < 9; col++)
			{
				if (int.TryParse(line[col].ToString(), out int value) && value is >= 1 and <= 9)
				{
					board[col][row] = value;
				}
				else
				{
					board[col][row] = Cell.EMPTY_VALUE; // Anything else can represent Cell.EMPTY_VALUE
				}
			}
		}

		return new Puzzle(board, false);
	}

	public void Reset()
	{
		for (int i = 0; i < 81; i++)
		{
			Cell cell = _board[i];
			if (cell.Value != cell.OriginalValue)
			{
				cell.Set(Cell.EMPTY_VALUE);
			}
		}
	}
	/// <summary>Returns true if any digit is repeated. Can be called even if the puzzle isn't solved yet.</summary>
	public bool CheckForErrors()
	{
		for (int digit = 1; digit <= 9; digit++)
		{
			for (int i = 0; i < 9; i++)
			{
				if (Blocks[i].CheckForDuplicateValue(digit)
					|| Rows[i].CheckForDuplicateValue(digit)
					|| Columns[i].CheckForDuplicateValue(digit))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		for (int row = 0; row < 9; row++)
		{
			for (int col = 0; col < 9; col++)
			{
				Cell cell = _board[Utils.CellIndex(col, row)];
				if (cell.OriginalValue == Cell.EMPTY_VALUE)
				{
					sb.Append('-');
				}
				else
				{
					sb.Append(cell.OriginalValue);
				}
			}
			if (row != 8)
			{
				sb.AppendLine();
			}
		}
		return sb.ToString();
	}
	public string ToStringFancy()
	{
		var sb = new StringBuilder();
		for (int row = 0; row < 9; row++)
		{
			if (row % 3 == 0)
			{
				for (int col = 0; col < 13; col++)
				{
					sb.Append('—');
				}
				sb.AppendLine();
			}
			for (int col = 0; col < 9; col++)
			{
				if (col % 3 == 0)
				{
					sb.Append('┃');
				}

				Cell cell = _board[Utils.CellIndex(col, row)];
				if (cell.Value == Cell.EMPTY_VALUE)
				{
					sb.Append(' ');
				}
				else
				{
					sb.Append(cell.Value);
				}

				if (col == 8)
				{
					sb.Append('┃');
				}
			}
			sb.AppendLine();
			if (row == 8)
			{
				for (int col = 0; col < 13; col++)
				{
					sb.Append('—');
				}
			}
		}
		return sb.ToString();
	}
}
