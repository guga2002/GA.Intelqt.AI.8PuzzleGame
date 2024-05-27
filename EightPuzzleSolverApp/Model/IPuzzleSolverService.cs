using System.Threading;
using System.Threading.Tasks;
using EightPuzzleSolver.EightPuzzle;

namespace EightPuzzleSolverApp.Model
{
    public interface IPuzzleSolverService
    {
        Task<SolutionSearchResult> SolveAsync(Board initialBoard, Algorithm algorithm,CancellationToken cancellationToken);
    }
}
