using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using EightPuzzleSolver.EightPuzzle;
using GalaSoft.MvvmLight;
using EightPuzzleSolverApp.Model;
using EightPuzzleSolverApp.View;
using GalaSoft.MvvmLight.Command;

namespace EightPuzzleSolverApp.ViewModel
{
    public enum WorkState
    {
        Idle,
        Searching,
        ShowingMoves,
        Stopping
    }

    public sealed class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly IPuzzleSolverService _puzzleSolverService;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public MainViewModel(IPuzzleSolverService puzzleSolverService, IDialogService dialogService)
        {
            _puzzleSolverService = puzzleSolverService;
            _dialogService = dialogService;

            if (IsInDesignMode)
            {
                SearchResult = new SolutionSearchResult(true, new List<EightPuzzleState>()
                {
                    new EightPuzzleState(null),
                    new EightPuzzleState(null, null, MoveDirection.Left),
                    new EightPuzzleState(null, null, MoveDirection.Bottom),
                    new EightPuzzleState(null, null, MoveDirection.Right),
                });
                CurrentMoveNumber = 1;
            }
        }

        public delegate void CreateBoardHandler(object sender, CreateBoardEventArgs e);

        public event CreateBoardHandler CreateBoard;

        public delegate void ShowMovesHandler(object sender, EventArgs e);

        public event ShowMovesHandler ShowMoves;

        private WorkState _state = WorkState.Idle;
        public WorkState State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                RaisePropertyChanged();

                SolveOrStopCommand.RaiseCanExecuteChanged();
                GenerateBoardCommand.RaiseCanExecuteChanged();
                FillBoardCommand.RaiseCanExecuteChanged();
                ShowMovesCommand.RaiseCanExecuteChanged();
            }
        }

        private int _rowCount = 3;
        public int RowCount
        {
            get { return _rowCount; }
            set
            {
                if (_rowCount == value)
                {
                    return;
                }

                _rowCount = value;
                RaisePropertyChanged();
            }
        }

        private int _columnCount = 3;
        public int ColumnCount
        {
            get { return _columnCount; }
            set
            {
                if (_columnCount == value)
                {
                    return;
                }

                _columnCount = value;
                RaisePropertyChanged();
            }
        }

        private string _boardInputText = "8 6 7\r\n2 5 4\r\n3 0 1";
        public string BoardInputText
        {
            get { return _boardInputText; }
            set
            {
                if (_boardInputText == value)
                {
                    return;
                }

                _boardInputText = value;
                RaisePropertyChanged();
            }
        }

        private Board _currentBoard;
        public Board CurrentBoard
        {
            get
            { return _currentBoard; }
            set
            {
                if (_currentBoard == value)
                {
                    return;
                }

                _currentBoard = value;
                RaisePropertyChanged();
            }
        }

        private SolutionSearchResult _searchResult;
        public SolutionSearchResult SearchResult
        {
            get
            { return _searchResult; }
            set
            {
                if (_searchResult == value)
                {
                    return;
                }

                _searchResult = value;
                RaisePropertyChanged();
            }
        }

        private int _currentMoveNumber = -1;

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        public int CurrentMoveNumber
        {
            get
            { return _currentMoveNumber; }
            set
            {
                if (_currentMoveNumber == value)
                {
                    return;
                }

                _currentMoveNumber = value;
                RaisePropertyChanged();

                RaisePropertyChanged(nameof(CurrentMoveIndex));
            }
        }

        public int CurrentMoveIndex => CurrentMoveNumber - 1;

        public static IList<Algorithm> Algorithms { get; } = new[]
        {
            Algorithm.AStar,
            Algorithm.RecursiveBestFirstSearch,
        };

        private Algorithm _selectedAlgorithm = Algorithms.First();
        public Algorithm SelectedAlgorithm
        {
            get
            { return _selectedAlgorithm; }
            set
            {
                if (_selectedAlgorithm == value)
                {
                    return;
                }

                _selectedAlgorithm = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _generateBoardCommand;
        public RelayCommand GenerateBoardCommand
        {
            get
            {
                return _generateBoardCommand
                       ?? (_generateBoardCommand = new RelayCommand(
                           GenerateBoard,
                           () => State == WorkState.Idle));
            }
        }

        private RelayCommand _solveOrStopCommand;

        public RelayCommand SolveOrStopCommand
        {
            get
            {
                return _solveOrStopCommand 
                       ?? (_solveOrStopCommand = new RelayCommand(
                           () =>
                           {
                               if (State == WorkState.Idle)
                               {
                                   Solve();
                               }
                               else
                               {
                                   Stop();
                               }
                           },
                           () => State == WorkState.Idle || State == WorkState.Searching || State == WorkState.ShowingMoves));
            }
        }

        private RelayCommand _fillBoardCommand;
        public RelayCommand FillBoardCommand
        {
            get
            {
                return _fillBoardCommand
                       ?? (_fillBoardCommand = new RelayCommand(
                           FillBoard,
                           () => State == WorkState.Idle));
            }
        }

        private RelayCommand _showMovesCommand;
        public RelayCommand ShowMovesCommand
        {
            get
            {
                return _showMovesCommand 
                       ?? (_showMovesCommand = new RelayCommand(
                           StartShowingMoves,
                           () => State == WorkState.Idle && SearchResult != null && SearchResult.Success));
            }
        }

        public EightPuzzleState NextMoveState()
        {
            if (CurrentMoveNumber + 1 > SearchResult.MoveCount || _cancellationToken.IsCancellationRequested)
            {
                State = WorkState.Idle;

                return null;
            }

            CurrentMoveNumber++;

            return SearchResult.Solution[CurrentMoveNumber];
        }

        private void GenerateBoard()
        {
            var board = Board.GenerateSolvableBoard(RowCount, ColumnCount);

            BoardInputText = BoardToText(board);

            FillBoard();
        }

        private void FillBoard()
        {
            Board board;

            try
            {
                board = TextToBoard(BoardInputText);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError("araswori inicializeba" + ex.Message);
                return;
            }

            if (!board.IsCorrect())
            {
                _dialogService.ShowError("mocemuli wyoba arasworia");
                return;
            }

            BoardInputText = BoardToText(board);

            CurrentBoard = board;

            OnCreateBoard(new CreateBoardEventArgs(board));
        }

        private async void Solve()
        {
            OnCreateBoard(new CreateBoardEventArgs(CurrentBoard));

            if (!CurrentBoard.IsSolvable())
            {
                if (!_dialogService.ShowConfirmation("mocemuli wyoba ar aris amoxsnadi"))
                    return;
            }

            CreateCancellationToken();

            SearchResult = null;

            State = WorkState.Searching;

            try
            {
                SearchResult = await _puzzleSolverService.SolveAsync(CurrentBoard, SelectedAlgorithm, _cancellationToken);

                if (!SearchResult.Success)
                {
                    _dialogService.ShowError("amoxsna ver vipovet!");
                }
                else
                {
                    StartShowingMoves();

                    return;
                }
            }
            catch(OperationCanceledException)
            { }
            catch (Exception ex)
            {
                _dialogService.ShowError(ex.Message);
            }

            State = WorkState.Idle;
        }

        private void StartShowingMoves()
        {
            OnCreateBoard(new CreateBoardEventArgs(CurrentBoard));

            CreateCancellationToken();

            State = WorkState.ShowingMoves;

            CurrentMoveNumber = 0;

            OnShowMoves();
        }

        private void Stop()
        {
            State = WorkState.Stopping;

            _cancellationTokenSource.Cancel();
        }

        private void CreateCancellationToken()
        {
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        private static string BoardToText(Board board)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < board.RowCount; i++)
            {
                for (int j = 0; j < board.ColumnCount; j++)
                {
                    sb.Append(board[i, j] + " ");
                }
                if (i < board.RowCount - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        private static Board TextToBoard(string str)
        {
            var lines = str.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);

            List<List<byte>> rows;
            try
            {
                rows = lines.Select(l => l.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(byte.Parse).ToList())
                                    .ToList();
            }
            catch (FormatException)
            {
                throw new Exception("araswori formati.");
            }

            int rowCount = rows.Count;
            int columnCount = rows.First().Count;

            if (rows.Any(r => r.Count != columnCount))
                throw new Exception("yvelas unda qondes erti zoma");

            var data = new byte[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    data[i, j] = rows[i][j];
                }
            }

            return new Board(data);
        }

        private void OnCreateBoard(CreateBoardEventArgs args)
        {
            CreateBoard?.Invoke(this, args);
        }

        private void OnShowMoves()
        {
            ShowMoves?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
            }
        }
    }
}