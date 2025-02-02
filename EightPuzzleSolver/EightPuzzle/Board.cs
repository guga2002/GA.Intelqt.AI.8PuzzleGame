﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EightPuzzleSolver.Helpers;

namespace EightPuzzleSolver.EightPuzzle
{
    public class Board
    {

        #region Properties
        private readonly byte[] _data;

        private Position? _blankTilePos;

        public int ColumnCount { get; }

        public byte RowCount { get; }
        #endregion

        #region Constructors
        public Board(byte[] data, int rowCount, int columnCount)
        {
            _data = data;
            RowCount = (byte)rowCount;
            ColumnCount = (byte)columnCount;
        }
        public Board(byte[,] data)
        {
            RowCount = (byte)data.GetLength(0);
            ColumnCount = (byte)data.GetLength(1);

            _data = new byte[ColumnCount * RowCount];

            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    _data[To1DCoord(i, j, ColumnCount)] = data[i, j];
                }
            }
        }
        private Board(byte[] data, int rowCount, int columnCount, Position blankTilePos)
            : this(data, rowCount, columnCount)
        {
            _blankTilePos = blankTilePos;

            Debug.Assert(this[_blankTilePos.Value] == 0);
        }
        #endregion

        #region Amoxsnadi cxrilis dagenerireba
        public static Board GenerateSolvableBoard(int rowCount, int columnCount)
        {
            while (true)
            {
                var data = Enumerable.Range(0, rowCount * columnCount)
                        .OrderBy(r => StaticRandom.Next())
                        .Select(val => (byte)val)
                        .ToArray();

                var board = new Board(data, rowCount, columnCount);

                Debug.Assert(board.IsCorrect());

                if (board.IsSolvable())
                    return board;
            }
        }

        #endregion

        #region Miznis  statis dagenerireba
        public static Board CreateGoalBoard(int rowCount, int columnCount)
        {
            var data = Enumerable.Range(1, rowCount * columnCount - 1)
                .Concat(new [] { 0 })
                .Select(val => (byte)val)
                .ToArray();

            var board = new Board(data, rowCount, columnCount);

            return board;
        }
        #endregion


        public byte this[int row, int col] => _data[To1DCoord(row, col, ColumnCount)];

        public byte this[Position pos] => this[pos.Row, pos.Column];

        public Position BlankTilePosition
        {
            get
            {
                if (!_blankTilePos.HasValue)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        for (int j = 0; j < ColumnCount; j++)
                        {
                            if (this[i, j] == 0)
                            {
                                _blankTilePos = new Position(i, j);
                                return _blankTilePos.Value;
                            }
                        }
                    }

                    throw new ArgumentException("blank tile is not found");
                }

                return _blankTilePos.Value;
            }
        }

        public bool CanMove(MoveDirection direction)//check if move posible
        {
            var newBlankTilePos = BlankTilePosition.Move(direction);

            return newBlankTilePos.Row >= 0 && newBlankTilePos.Row < RowCount &&
                newBlankTilePos.Column >= 0 && newBlankTilePos.Column < ColumnCount;
        }

        public Board Move(MoveDirection direction)//make move
        {
            if (!CanMove(direction))
                throw new ArgumentException("Mimartuleba arasworia");

            var newBlankTilePos = BlankTilePosition.Move(direction);

            var newData = (byte[]) _data.Clone();
            newData[To1DCoord(BlankTilePosition, ColumnCount)] = newData[To1DCoord(newBlankTilePos, ColumnCount)];
            newData[To1DCoord(newBlankTilePos, ColumnCount)] = 0;

            return new Board(newData, RowCount, ColumnCount, newBlankTilePos);
        }

        public bool IsSolvable()//vamowmebt tu aris amoxsnadi
        {
            if (!IsCorrect())
            {
                return false;
            }

            int inversionCount = 0;

            for (int i = 0; i < _data.Length - 1; i++)
            {
                for (int j = i + 1; j < _data.Length; j++)
                {
                    if (_data[j] > 0 && _data[i] > _data[j])
                        inversionCount++;
                }
            }

            if (IsOdd(ColumnCount))
            {
                return IsEven(inversionCount);
            }
            else
            {
                int blankRowFromBottom = RowCount - BlankTilePosition.Row;

                if (IsEven(blankRowFromBottom))
                    return IsOdd(inversionCount);
                else
                {
                    return IsEven(inversionCount);
                }
            }
        }

        public bool IsCorrect()
        {
            if (_data.Distinct().Count() != _data.Length)
            {
                return false;
            }
            if (_data.Any(val => val > RowCount * ColumnCount - 1))
            {
                return false;
            }

            return true;
        }

        protected bool Equals(Board other)
        {
            if (other.ColumnCount != this.ColumnCount || other.RowCount != this.RowCount)
                return false;

            return other._data.SequenceEqual(_data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Board) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 42;
                for (int i = 0; i < _data.Length; i++)
                {
                    hashCode = hashCode * 17 + _data[i];
                }
                hashCode = hashCode * 397 + ColumnCount;
                hashCode = hashCode * 397 + RowCount;
                return hashCode;
            }
        }

        public static bool operator ==(Board left, Board right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Board left, Board right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            for (int i = 0; i < RowCount; i++)
            {
                sb.Append("{ ");
                for (int j = 0; j < ColumnCount; j++)
                {
                    sb.Append(this[i, j]);
                    sb.Append(" ");
                }
                sb.Append("}");
                if (i < RowCount - 1)
                    sb.AppendLine(", ");
            }
            return $"{RowCount}x{ColumnCount}: {sb}";
        }

        private static int To1DCoord(int row, int col, int width)
        {
            return row*width + col;
        }

        private static int To1DCoord(Position pos, int width)
        {
            return pos.Row * width + pos.Column;
        }

        private static bool IsEven(int num)
        {
            return num % 2 == 0;
        }

        private static bool IsOdd(int num)
        {
            return num % 2 == 1;
        }
    }
}
