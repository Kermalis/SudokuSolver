using System;
using System.Collections.ObjectModel;
using System.Linq;
using Kermalis.SudokuSolver.Core;

namespace SudokuSolver.Core
{
    class SearchStrategy
    {
        
        public Puzzle Result { get; set; }
        private readonly Cell[] _configurationArray;
        public SearchStrategy()
        {
            
            _configurationArray = new Cell[9 * 9];
        }

        public bool SolvePuzzle(Puzzle puzzle)
        {
            var initialDepth = InitializeConfigurationArray(puzzle);
            return DepthFirstSearch(puzzle.Clone(),initialDepth);
        }
        

        public int InitializeConfigurationArray(Puzzle puzzle)
        {
            var k = 0;
            for (var i = 0; i < 9; i++)
            for (var j = 0; j < 9; j++)
            {
                if (puzzle[i, j].HasMoreThanOneCandidate)
                {
                    _configurationArray[k++] = puzzle[i, j];
                }
            }
            Array.Sort(_configurationArray);
            return _configurationArray.Length - k;
        }

        public bool DepthFirstSearch(Puzzle puzzle,int depth)
        {
            if (!IsOkay(puzzle)) return false;

            if (depth == _configurationArray.Length)
            {
                Result = puzzle;
                return true;
            }

            var nextCell = _configurationArray[depth];

            foreach (var candid in nextCell.Candidates.ToList())
            {
                var clone = puzzle.Clone();
                var cell = clone[nextCell.Point.X,nextCell.Point.Y];
                cell.Set(candid);

                if (DepthFirstSearch(clone, depth + 1))
                    return true;
            }

            return false;
        }

        public bool IsOkay(Puzzle puzzle)
        {
            foreach (ReadOnlyCollection<Region> regionCollection in puzzle.Regions)
            {
                foreach (Region region in regionCollection)
                {
                    if (region.HasProblem()) return false;
                }
            }

            return true;
        }

        
    }
}
