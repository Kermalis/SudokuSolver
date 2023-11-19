using System;
using System.ComponentModel;
using Xunit;
using Xunit.Abstractions;

namespace Kermalis.SudokuSolver.Tests;

[CollectionDefinition("Utils")]
public sealed class TestUtilsCollection : ICollectionFixture<TestUtils>
{
	//
}

public sealed class TestUtils
{
	private ITestOutputHelper _output = null!;

	public void SetOutputHelper(ITestOutputHelper output)
	{
		_output = output;
	}

	public Solver CreateSolver(ReadOnlySpan<string> puzzleText)
	{
		var solver = new Solver(Puzzle.Parse(puzzleText));
		_output.WriteLine(solver.Puzzle.ToStringFancy());
		_output.WriteLine(string.Empty);
		solver.Actions.ListChanged += Actions_ListChanged;
		return solver;
	}
	public void AssertSolvedCorrectly(Solver solver)
	{
		Assert.True(solver.TrySolve());
		Assert.False(solver.Puzzle.CheckForErrors());

		_output.WriteLine(string.Empty);
		_output.WriteLine(solver.Puzzle.ToStringFancy());
	}

	private void Actions_ListChanged(object? sender, ListChangedEventArgs e)
	{
		if (e.ListChangedType == ListChangedType.ItemAdded)
		{
			var list = (BindingList<string>)sender!;
			_output.WriteLine(list[e.NewIndex]);
		}
	}

	private void Actions_AddingNew(object? sender, AddingNewEventArgs e)
	{
	}
}