using EightPuzzleSolver.Search.Algorithms;

namespace EightPuzzleSolver.Search
{
    public abstract class Problem<TProblemState> where TProblemState : IProblemState<TProblemState>
    {
        protected Problem(TProblemState initialState)
        {
            InitialState = initialState;
        }

        public TProblemState InitialState { get; }
        public abstract bool IsGoalState(TProblemState state);

    }
}
