using System.ComponentModel;
using System.Linq;

namespace SudokuSolver.Core
{
    internal class Solver
    {
        internal readonly Puzzle Puzzle;

        internal Solver(int[][] inBoard, bool bCustom) => TaskForce.Init(Logger.Puzzle = Puzzle = new Puzzle(inBoard, bCustom));

        internal void DoWork(object sender, DoWorkEventArgs e)
        {
            Puzzle.RefreshCandidates();
            Logger.Log("Begin");

            bool solved; // If this is true after a segment, the puzzle is solved and we can break

            do
            {
                solved = true;

                bool changed = false;
                // Check for naked singles or a completed puzzle
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        Cell c = Puzzle[x, y];
                        if (c != 0) continue;

                        solved = false;
                        // Check for naked singles
                        var a = c.Candidates.ToArray(); // Copy
                        if (a.Length == 1)
                        {
                            c.Set(a[0]);
                            Logger.Log("Naked single", new Cell[] { c }, a);
                            changed = true;
                        }
                    }
                }
                if (solved) break;
                if (changed || TaskForce.Execute())
                    continue;

                break;
            } while (true);

            e.Result = solved;
        }
    }
}
