namespace Kermalis.SudokuSolver;

public sealed class PuzzleSnapshot
{
	public string Action { get; }
	/// <summary>Stored as x,y (col,row)</summary>
	private readonly CellSnapshot[] _board;

	public CellSnapshot this[int col, int row] => _board[Utils.CellIndex(col, row)];

	internal PuzzleSnapshot(string action, CellSnapshot[] board)
	{
		Action = action;
		_board = board;
	}

	public override string ToString()
	{
		return Action;
	}
}