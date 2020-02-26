using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kermalis.SudokuSolver.Core
{
    class Region
    {
        public ReadOnlyCollection<Cell> Cells { get; }

        public Region(Cell[] cells)
        {
            Cells = new ReadOnlyCollection<Cell>(cells);
        }

        public IEnumerable<Cell> GetCellsWithCandidates(params int[] candidates)
        {
            return Cells.Where(c => c.Candidates.ContainsAll(candidates));
        }

        public bool HasProblem()
        {
            HashSet<int> hashSet = new HashSet<int>();
            foreach (Cell cell in Cells)
            {
                if (cell.Value == 0) continue;
                if (!hashSet.Add(cell.Value))
                    return false;
            }

            return true;
        }
    }
}
