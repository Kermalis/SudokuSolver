using Xunit;
using Xunit.Abstractions;

namespace Kermalis.SudokuSolver.Tests;

[Collection("Utils")]
public sealed class TestSolverTechniques
{
	private readonly TestUtils _utils;

	public TestSolverTechniques(TestUtils utils, ITestOutputHelper output)
	{
		_utils = utils;
		_utils.SetOutputHelper(output);
	}

	[Fact]
	public void Jellyfish()
	{
		string[] puzzleText =
		[
			"2-------3",
			"-8--3--5-",
			"--34-21--",
			"--12-54--",
			"----9----",
			"--93-86--",
			"--25-69--",
			"-9--2--7-",
			"4-------1"
		];

		Solver solver = _utils.CreateSolver(puzzleText);
		_utils.AssertSolvedCorrectly(solver);
	}
}