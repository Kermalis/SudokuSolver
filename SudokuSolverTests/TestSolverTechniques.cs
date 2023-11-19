using System;
using Xunit;
using Xunit.Abstractions;

namespace Kermalis.SudokuSolver.Tests;

[Collection(TestUtilsCollection.DEF)]
public sealed class TestSolverTechniques
{
	private readonly TestUtils _utils;

	public TestSolverTechniques(TestUtils utils, ITestOutputHelper output)
	{
		_utils = utils;
		_utils.SetOutputHelper(output);
	}

	private void SolveBasic(string technique, ReadOnlySpan<string> puzzleText)
	{
		Solver solver = _utils.CreateSolver(puzzleText);
		_utils.AssertSolvedCorrectly(solver, technique);
	}

	// TODO: Avoidable Rectangle, Hidden Pair, Locked Candidate, Naked Quadruple, Naked Pair, Swordfish, Unique Rectangle (all kinds), X-Wing, XYZ-Wing

	[Fact]
	public void HiddenRectangle()
	{
		SolveBasic("Hidden rectangle", [
			"----5-2--",
			"3--7-----",
			"7---9--1-",
			"9--1--746",
			"---------",
			"---5----9",
			"-29---4--",
			"-6---4---",
			"-8---23-1"
		]);
	}

	[Fact]
	public void HiddenQuadruple()
	{
		SolveBasic("Hidden quadruple", [
			"-3-----1-",
			"--8-9----",
			"4--6-8---",
			"---57694-",
			"---98352-",
			"---124---",
			"276--519-",
			"---7-9---",
			"-95---47-"
		]);
	}
	[Fact]
	public void HiddenTriple()
	{
		SolveBasic("Hidden triple", [
			"4-7-----5",
			"---2--7--",
			"2-1-7-6-8",
			"--91-23--",
			"3-2-97---",
			"17--6----",
			"72-851--6",
			"986734---",
			"51-629---"
		]);
	}

	[Fact]
	public void Jellyfish()
	{
		SolveBasic("Jellyfish", [
			"2-------3",
			"-8--3--5-",
			"--34-21--",
			"--12-54--",
			"----9----",
			"--93-86--",
			"--25-69--",
			"-9--2--7-",
			"4-------1"
		]);
	}

	[Fact]
	public void NakedTriple()
	{
		SolveBasic("Naked triple", [
			"891--576-",
			"53769-2-8",
			"462-----5",
			"24351-8-6",
			"156---4--",
			"978-465--",
			"319-5-687",
			"684----52",
			"72586-3--"
		]);
	}

	[Fact]
	public void PointingTuple()
	{
		SolveBasic("Pointing tuple", [
			"-32--61--",
			"41-------",
			"---9-1---",
			"5---9---4",
			"-6-----7-",
			"3---2---5",
			"---5-8---",
			"-------19",
			"--7---86-"
		]);
	}

	[Fact]
	public void XYChain()
	{
		SolveBasic("XY-Chain", [
			"--3--1---",
			"8--------",
			"-51--9-6-",
			"-8----29-",
			"---7---8-",
			"2---4-5-3",
			"6--9-----",
			"--2-84---",
			"41--5-6--"
		]);
	}

	[Fact]
	public void YWing()
	{
		SolveBasic("Y-Wing", [
			"9---4----",
			"---6---31",
			"-2-----9-",
			"---7---2-",
			"--29356--",
			"-7---2---",
			"-6-----73",
			"51---9---",
			"----8---9"
		]);
	}
}