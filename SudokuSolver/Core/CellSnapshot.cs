using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kermalis.SudokuSolver.Core;

internal sealed class CellSnapshot
{
	public int Value { get; }
	public ReadOnlyCollection<int> Candidates { get; }
	public bool IsCulprit { get; }
	public bool IsSemiCulprit { get; }

	public CellSnapshot(int value, HashSet<int> candidates, bool isCulprit, bool isSemiCulprit)
	{
		Value = value;
		Candidates = new ReadOnlyCollection<int>(candidates.ToArray());
		IsCulprit = isCulprit;
		IsSemiCulprit = isSemiCulprit;
	}
}