using System;

namespace Kermalis.SudokuSolver.Core;

internal sealed partial class Solver
{
	private sealed class SolverTechnique
	{
		public Func<Puzzle, bool> Function { get; }
		/// <summary>Currently unused.</summary>
		public string Url { get; }

		public SolverTechnique(Func<Puzzle, bool> function, string url)
		{
			Function = function;
			Url = url;
		}
	}

	private static readonly SolverTechnique[] _techniques = new[]
	{
		new SolverTechnique(HiddenSingle, "Hidden single"),
		new SolverTechnique(NakedPair, "https://hodoku.sourceforge.net/en/tech_naked.php#n2"),
		new SolverTechnique(HiddenPair, "https://hodoku.sourceforge.net/en/tech_hidden.php#h2"),
		new SolverTechnique(LockedCandidate, "https://hodoku.sourceforge.net/en/tech_intersections.php#lc1"),
		new SolverTechnique(PointingTuple, "https://hodoku.sourceforge.net/en/tech_intersections.php#lc1"),
		new SolverTechnique(NakedTriple, "https://hodoku.sourceforge.net/en/tech_naked.php#n3"),
		new SolverTechnique(HiddenTriple, "https://hodoku.sourceforge.net/en/tech_hidden.php#h3"),
		new SolverTechnique(XWing, "https://hodoku.sourceforge.net/en/tech_fishb.php#bf2"),
		new SolverTechnique(Swordfish, "https://hodoku.sourceforge.net/en/tech_fishb.php#bf3"),
		new SolverTechnique(YWing, "https://www.sudokuwiki.org/Y_Wing_Strategy"),
		new SolverTechnique(XYZWing, "https://www.sudokuwiki.org/XYZ_Wing"),
		new SolverTechnique(XYChain, "https://www.sudokuwiki.org/XY_Chains"),
		new SolverTechnique(NakedQuadruple, "https://hodoku.sourceforge.net/en/tech_naked.php#n4"),
		new SolverTechnique(HiddenQuadruple, "https://hodoku.sourceforge.net/en/tech_hidden.php#h4"),
		new SolverTechnique(Jellyfish, "https://hodoku.sourceforge.net/en/tech_fishb.php#bf4"),
		new SolverTechnique(UniqueRectangle, "https://hodoku.sourceforge.net/en/tech_ur.php"),
		new SolverTechnique(HiddenRectangle, "https://hodoku.sourceforge.net/en/tech_ur.php#hr"),
		new SolverTechnique(AvoidableRectangle, "https://hodoku.sourceforge.net/en/tech_ur.php#ar"),
	};
}
