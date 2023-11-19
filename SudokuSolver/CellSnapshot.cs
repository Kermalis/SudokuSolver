using System.Collections.ObjectModel;

namespace Kermalis.SudokuSolver;

public sealed class CellSnapshot
{
	public int Value { get; }
	public ReadOnlyCollection<int> Candidates { get; }
	public bool IsCulprit { get; }
	public bool IsSemiCulprit { get; }

	internal CellSnapshot(int value, ReadOnlyCollection<int> candidates, bool isCulprit, bool isSemiCulprit)
	{
		Value = value;
		Candidates = candidates;
		IsCulprit = isCulprit;
		IsSemiCulprit = isSemiCulprit;
	}
}