using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Kermalis.SudokuSolver.Core;

namespace SudokuSolver.Core
{
    class SearchStrategy
    {
        private readonly Puzzle _puzzle;

        private Cell[] _configurationArray;
        public SearchStrategy(Puzzle puzzle)
        {
            _puzzle = puzzle;
            _configurationArray = new Cell[9 * 9];
        }

        public bool SolvePuzzle()
        {
            var initialDepth = InitializeConfigurationArray();
            return DepthFirstSearch(initialDepth);
        }

        public int InitializeConfigurationArray()
        {
            int k = 0;
            for (var i = 0; i < 9; i++)
            for (var j = 0; j < 9; j++)
            {
                if (_puzzle[i, j].HasMoreThanOneCandidate())
                {
                    _configurationArray[k++] = _puzzle[i, j];
                }
            }
            Array.Sort(_configurationArray);
            return _configurationArray.Length - k;
        }

        public bool DepthFirstSearch(int depth)
        {
            if (depth == _configurationArray.Length)
            {
                return IsOkay();
            }

            var nextCell = _configurationArray[depth];
            foreach (var candid in nextCell.Candidates)
            {
                nextCell.Set(candid);
            }

            return true;
        }

        public bool IsOkay()
        {
            foreach (ReadOnlyCollection<Region> regionCollection in _puzzle.Regions)
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
