using System;
using System.Collections.Generic;

namespace EightPuzzleSolver.Search
{
    public interface IProblemState<TProblemState> : IEquatable<TProblemState>
                                        where TProblemState : IProblemState<TProblemState>
    {
        int Cost { get; }
        ISet<TProblemState> NextStates();
    }
}
