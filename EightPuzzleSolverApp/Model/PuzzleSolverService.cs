using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EightPuzzleSolver.EightPuzzle;
using EightPuzzleSolver.Search;
using EightPuzzleSolver.Search.Algorithms;

namespace EightPuzzleSolverApp.Model
{
    public class PuzzleSolverService : IPuzzleSolverService
    {
 

        public async Task<SolutionSearchResult> SolveAsync(Board initialBoard, Algorithm algorithm, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                return Solve(initialBoard, algorithm, cancellationToken);
            }, cancellationToken);
        }


        #region Private helper functions
        private SolutionSearchResult Solve(Board initialBoard, Algorithm algorithm, CancellationToken cancellationToken)
        {
            var problem = new EightPuzzleProblem(initialBoard);
            var search = CreateSearch(initialBoard, algorithm);
            var result = search.Search(problem, cancellationToken).ToList();
            return new SolutionSearchResult(result.Any(), result);
        }

        private ISearch<EightPuzzleState> CreateSearch(Board initialBoard, Algorithm algorithm)
        {
                IHeuristicFunction<EightPuzzleState> h;
                var goalBoard = Board.CreateGoalBoard(initialBoard.RowCount, initialBoard.ColumnCount);
                h = new ManhattanHeuristicFunction(goalBoard);
            switch (algorithm)
            {
                case Algorithm.AStar:
                    return new AStarSearch<EightPuzzleState>(h);// A star algorithm
                case Algorithm.RecursiveBestFirstSearch:
                    return new RecursiveBestFirstSearch<EightPuzzleState>(h);// DFS algorithm
                default:
                    return null;
            }
        }
        #endregion
    }
}