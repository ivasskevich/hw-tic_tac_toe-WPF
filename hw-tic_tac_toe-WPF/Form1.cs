namespace hw_tic_tac_toe_WPF
{
    public partial class GameForm : Form, ITicTacToeView
    {
        private Button[,] _buttons;
        public event Action<int, int> CellClicked;
        public event Action StartGame;

        public GameForm()
        {
            InitializeComponent();
            CreateGameBoard();

            button10.Click += (s, e) => StartGame?.Invoke();

            EnableControls(false);
        }

        private void CreateGameBoard()
        {
            _buttons = new Button[3, 3]
            {
                { button1, button2, button3 },
                { button4, button5, button6 },
                { button7, button8, button9 }
            };

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int r = row, c = col;
                    _buttons[row, col].Click += (s, e) => CellClicked?.Invoke(r, c);
                }
            }
        }

        public void UpdateBoard(string[,] board)
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    _buttons[row, col].Text = board[row, col];
                }
            }
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message, "Game Result");
        }

        public void EnableControls(bool enabled)
        {
            foreach (var button in _buttons)
            {
                button.Enabled = enabled;
            }
        }

        public void HighlightCell(int row, int col)
        {
            _buttons[row, col].BackColor = Color.LightBlue;
        }

        public bool IsComputerFirst() => checkBox1.Checked;
        public bool IsEasyMode() => radioButton1.Checked;
    }

    public interface ITicTacToeView
    {
        void UpdateBoard(string[,] board);
        void ShowMessage(string message);
        void EnableControls(bool enabled);
        void HighlightCell(int row, int col);
        bool IsComputerFirst();
        bool IsEasyMode();

        event Action<int, int> CellClicked;
        event Action StartGame;
    }

    public class TicTacToeModel
    {
        public string[,] Board { get; private set; }
        public bool IsPlayerTurn { get; set; }
        public bool IsGameOver { get; set; }
        public string PlayerSymbol { get; } = "X";
        public string ComputerSymbol { get; } = "O";

        public TicTacToeModel()
        {
            Board = new string[3, 3];
            Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Board[i, j] = string.Empty;
                }
            }

            IsPlayerTurn = true;
            IsGameOver = false;
        }

        public bool CheckWinner(string symbol)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Board[i, 0] == symbol && Board[i, 1] == symbol && Board[i, 2] == symbol) return true;
                if (Board[0, i] == symbol && Board[1, i] == symbol && Board[2, i] == symbol) return true;
            }
            if (Board[0, 0] == symbol && Board[1, 1] == symbol && Board[2, 2] == symbol) return true;
            if (Board[0, 2] == symbol && Board[1, 1] == symbol && Board[2, 0] == symbol) return true;

            return false;
        }

        public bool IsDraw()
        {
            foreach (var cell in Board)
            {
                if (string.IsNullOrEmpty(cell))
                    return false;
            }
            return true;
        }

        public void MakeMove(int row, int col, string symbol)
        {
            Board[row, col] = symbol;
        }
    }

    public class TicTacToePresenter
    {
        private readonly ITicTacToeView _view;
        private readonly TicTacToeModel _model;

        public TicTacToePresenter(ITicTacToeView view, TicTacToeModel model)
        {
            _view = view;
            _model = model;

            _view.CellClicked += OnCellClicked;
            _view.StartGame += StartGame;
        }

        private void StartGame()
        {
            _model.Reset();
            _view.UpdateBoard(_model.Board);
            _view.EnableControls(true);

            if (_view.IsComputerFirst())
            {
                MakeComputerMove();
            }
        }

        private void OnCellClicked(int row, int col)
        {
            if (_model.IsGameOver || !string.IsNullOrEmpty(_model.Board[row, col])) return;

            _model.MakeMove(row, col, _model.PlayerSymbol);
            _view.UpdateBoard(_model.Board);

            if (CheckGameOver(_model.PlayerSymbol)) return;

            MakeComputerMove();
        }

        private void MakeComputerMove()
        {
            if (_model.IsGameOver) return;

            var bestMove = _view.IsEasyMode() ? GetRandomMove() : GetBestMove();
            if (bestMove.HasValue)
            {
                _model.MakeMove(bestMove.Value.Item1, bestMove.Value.Item2, _model.ComputerSymbol);
                _view.UpdateBoard(_model.Board);

                CheckGameOver(_model.ComputerSymbol);
            }
        }

        private bool CheckGameOver(string symbol)
        {
            if (_model.CheckWinner(symbol))
            {
                _model.IsGameOver = true;
                _view.ShowMessage($"{symbol} wins!");
                _view.EnableControls(false);
                return true;
            }

            if (_model.IsDraw())
            {
                _model.IsGameOver = true;
                _view.ShowMessage("It's a draw!");
                _view.EnableControls(false);
                return true;
            }

            return false;
        }

        private (int, int)? GetBestMove()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (string.IsNullOrEmpty(_model.Board[row, col]))
                    {
                        _model.Board[row, col] = _model.ComputerSymbol;
                        if (_model.CheckWinner(_model.ComputerSymbol))
                        {
                            _model.Board[row, col] = string.Empty;
                            return (row, col);
                        }
                        _model.Board[row, col] = string.Empty;
                    }
                }
            }

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (string.IsNullOrEmpty(_model.Board[row, col]))
                    {
                        _model.Board[row, col] = _model.PlayerSymbol;
                        if (_model.CheckWinner(_model.PlayerSymbol))
                        {
                            _model.Board[row, col] = string.Empty;
                            return (row, col);
                        }
                        _model.Board[row, col] = string.Empty;
                    }
                }
            }

            return GetRandomMove();
        }

        private (int, int)? GetRandomMove()
        {
            var random = new Random();
            var emptyCells = new List<(int, int)>();

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (string.IsNullOrEmpty(_model.Board[row, col]))
                    {
                        emptyCells.Add((row, col));
                    }
                }
            }

            return emptyCells.Count > 0 ? emptyCells[random.Next(emptyCells.Count)] : null;
        }
    }
}
