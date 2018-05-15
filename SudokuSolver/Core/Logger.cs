using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SudokuSolver.Core
{
    internal class Logger
    {
        internal static BindingList<string> Actions;
        internal static Puzzle Puzzle;

        internal static void Log(string technique, IEnumerable<SPoint> culprits, IEnumerable<int> candidates) => Log(technique, culprits, "{0}: {1}", culprits.Count() == 1 ? culprits.ElementAt(0).ToString() : culprits.Print(), candidates.Count() == 1 ? candidates.ElementAt(0).ToString() : candidates.Print());
        internal static void Log(string technique, IEnumerable<Cell> culprits, IEnumerable<int> candidates) => Log(technique, culprits.Select(c => c.Point), candidates);
        internal static void Log(string technique, IEnumerable<SPoint> culprits, string format, params object[] args) => Log(technique, culprits.Select(p => Puzzle[p]), format, args);
        internal static void Log(string technique, IEnumerable<Cell> culprits, string format, params object[] args) => Log(string.Format($"{technique,-20}" + format, args), culprits.ToArray());
        internal static void Log(string s, params Cell[] culprits)
        {
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    Puzzle[x, y].TakeSnapshot(culprits != null && culprits.Contains(Puzzle[x, y]));
            Actions.Add(s);
        }
    }
}
