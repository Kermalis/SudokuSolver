namespace Kermalis.SudokuSolver;

public readonly struct CellSnapshot
{
	public Candidates Candidates { get; }
	/// <summary>First 4 bits for <see cref="Value"/> [0, 9]. Next bit for <see cref="IsCulprit"/>. Next bit for <see cref="IsSemiCulprit"/>. Last 2 bits unused.</summary>
	private readonly byte _data;

	public int Value => _data & 0b0000_1111;
	public bool IsCulprit => (_data & 0b0001_0000) != 0;
	public bool IsSemiCulprit => (_data & 0b0010_0000) != 0;

	internal CellSnapshot(Cell cell, bool isCulprit, bool isSemiCulprit)
	{
		Candidates = cell.Candidates;
		_data = (byte)cell.Value;
		_data |= (byte)((isCulprit ? 1 : 0) << 4);
		_data |= (byte)((isSemiCulprit ? 1 : 0) << 5);
	}
}