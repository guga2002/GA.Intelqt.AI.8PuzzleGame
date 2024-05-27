using System;
using System.Collections.Generic;
using System.Linq;
using EightPuzzleSolver.EightPuzzle;

namespace EightPuzzleSolverApp.Model
{
    public class SolutionAction
    {
        public SolutionAction(MoveDirection direction)
        {
            Direction = direction;
        }

        public MoveDirection Direction { get; }

        public override string ToString()
        {
            return Direction.Name;
        }
    }

    public class SolutionSearchResult
    {
        public SolutionSearchResult(bool success, IList<EightPuzzleState> solution)
        {
            Success = success;
            Solution = solution;
        }

        public bool Success { get; }

        public IList<EightPuzzleState> Solution { get; }

        public int MoveCount => Solution?.Count - 1 ?? -1;

    }
}
