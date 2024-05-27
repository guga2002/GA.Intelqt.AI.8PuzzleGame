namespace EightPuzzleSolver.EightPuzzle
{
    public struct Position
    {
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; }
        public int Column { get; }

        public Position Move(MoveDirection direction)
        {
            return new Position(Row + direction.RowChange, Column + direction.ColumnChange);
        }

    }
}
