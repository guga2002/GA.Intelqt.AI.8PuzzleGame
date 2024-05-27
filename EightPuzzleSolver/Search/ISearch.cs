using System.Collections.Generic;
using System.Threading;

namespace EightPuzzleSolver.Search
{
    public interface ISearch<TProblemState> where TProblemState : IProblemState<TProblemState>
    {
        IEnumerable<TProblemState> Search(Problem<TProblemState> problem, CancellationToken cancellationToken = default(CancellationToken));
    }
}
