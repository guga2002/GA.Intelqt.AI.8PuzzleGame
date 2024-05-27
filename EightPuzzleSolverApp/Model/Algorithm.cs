using System.ComponentModel;

namespace EightPuzzleSolverApp.Model
{
    public enum Algorithm// sia algoritmebis romelsac viyenebt
    {
        [Description("A*")]
        AStar,
        [Description("განივი ძებნის ალგორითმი")]
        RecursiveBestFirstSearch,
    }
}
