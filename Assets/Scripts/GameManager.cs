using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;

namespace game2048
{
    public class GameManager : MonoBehaviour
    {   
        [SerializeField]
        private GameObject _tile, _tilePlaceholder, _boardObj, _tilesObj, _gameObj, _menuObj, _gameOverObj, _gameWonObj;
        [SerializeField]
        private TMPro.TextMeshProUGUI _scoreText, _bestScoreText;
        private int _bRow;
        private int _bCol;
        private bool _isAnimating = false;
        private bool _isWin = false;
        private Tile[,] _board { get; set; }
        private int[,] _boardState { get; set; }

        private List<int[,]> _boardStateHistory = new List<int[,]>();
        private readonly System.Random random = new System.Random();
        private int _boardLength;
        private PlayerInput _playerInput;

        private InputAction _keyboardAction;
        private InputAction _touchAction;
        private InputAction _touchPositionAction;
        private Vector2 _touchStartPos;
        private float _touchStartTime;
        private Vector2 _touchEndPos;
        private float _touchEndTime;
        [SerializeField]
        private float _touchMinimumDistance = .2f;
        [SerializeField]
        private float _touchMaximumTime = 1f;
        [SerializeField]
        private float _touchDirectionThreshold = .9f;


        private int _score;
        private int Score
        {
            get
            {
                return _score;
            }
            set
            {
                if (value != _score)
                {
                    _score = value;
                    _scoreText.text = _score.ToString();
                }
 
            }
        }
        private int _bestScore;
        private int BestScore
        {
            get
            {
                return _bestScore;
            }
            set
            {
                if (value != _bestScore)
                {
                    _bestScore = value;
                    _bestScoreText.text = _bestScore.ToString();
                }

            }
        }
        private GameState _gState = GameState.None;
        private GameState _gameState
        {
            get
            {
                return _gState;
            }
            set
            {
                if (value != _gState)
                {
                    switch (value)
                    {
                        case GameState.GameMenu:
                            GameMenu();
                            break;
                        case GameState.GameStart:
                            GameReset();
                            GameStart();
                            PutNewValue();
                            break;
                        case GameState.GameOver:
                            GameOver();
                            break;
                        case GameState.GameWon:
                            GameWon();
                            break;
                    }
                    _gState = value;
                }
            }
        }


        enum GameState
        {
            None,
            GameMenu,
            GameStart,
            GameOver,
            GameWon,
        }
        enum Direction
        {
            Up,
            Down,
            Right,
            Left,
        }

        void Start()
        {
            _gameState = GameState.GameMenu;
        }

        void GameMenu()
        {
            _gameObj.SetActive(false);
            _gameWonObj.SetActive(false);
            _gameOverObj.SetActive(false);
            _menuObj.SetActive(true);
            _menuObj.transform.localPosition = new Vector3(0, 645, 0);
            Sequence newSeq = DOTween.Sequence();
            newSeq.Append(_menuObj.transform.DOLocalMove(new Vector3(0, 0, 0), 0.5f));
            newSeq.Play();
        }

        void GameWon()
        {

            if (Score > BestScore)
            {
                PlayerPrefs.SetInt("bestScore", Score);
            }
            _menuObj.SetActive(false);
            _gameOverObj.SetActive(false);
            _gameWonObj.SetActive(true);
            _gameWonObj.transform.localPosition = new Vector3(0, 645, 0);
            Sequence seq = DOTween.Sequence();
            seq.Append(_gameWonObj.transform.DOLocalMove(new Vector3(0, 0, 0), 0.5f));
            seq.Play();
        }

        void GameOver()
        {

            if (Score > BestScore)
            {
                PlayerPrefs.SetInt("bestScore", Score);
            }
            _menuObj.SetActive(false);
            _gameWonObj.SetActive(false);
            _gameOverObj.SetActive(true);
            _gameOverObj.transform.localPosition = new Vector3(0, 645, 0);
            Sequence seq = DOTween.Sequence();
            seq.Append(_gameOverObj.transform.DOLocalMove(new Vector3(0, 0, 0), 0.5f));
            seq.Play();
        }

        public void SetBoardLength(int boardLength)
        {
            _boardLength = boardLength;
            _gameState = GameState.GameStart;
        }

        private void GameStart(int[,] initGameState = null)
        {
            BestScore = PlayerPrefs.GetInt("bestScore", 0);
            _gameObj.SetActive(true);
            _gameWonObj.SetActive(false);
            _gameOverObj.SetActive(false);
            _menuObj.SetActive(false);
            for (var i = _tilesObj.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_tilesObj.transform.GetChild(i).gameObject);
            }
            for (var i = _boardObj.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_boardObj.transform.GetChild(i).gameObject);
            }
            _tilesObj.GetComponent<GridLayoutGroup>().enabled = true;
            var cellSize = (400 - (_boardLength + 1) * 10) / _boardLength;
            _tilesObj.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);
            _boardObj.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);

            _board = new Tile[_boardLength, _boardLength];
            _bRow = _board.GetLength(0);
            _bCol = _board.GetLength(1);
            for (int i = 0; i < _bRow; i++)
            {
                for (int j = 0; j < _bCol; j++)
                {
                    Instantiate(_tilePlaceholder, _boardObj.transform);
                    var gObj = Instantiate(_tile, _tilesObj.transform);
                    _board[i, j] = gObj.GetComponent<Tile>();
                    if (initGameState != null)
                    {
                        if (initGameState[i, j] != 0)
                        {
                            _board[i, j].SetValue(initGameState[i, j]);
                            Sequence seq = DOTween.Sequence();
                            _board[i, j].PutNewValue(seq);
                            seq.Play();
                        }
                    }
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tilesObj.GetComponent<RectTransform>());
            foreach (Tile tile in _board)
            {
                tile.Position = tile.transform.position;
            }

            _tilesObj.GetComponent<GridLayoutGroup>().enabled = false;
        }

        private void GameReset()
        {
            Score = 0;
            _isWin = false;
            _boardStateHistory.Clear();
        }

        public void RotateMatrixByKTimes(Tile[,] matrix, int numberOftimes)
        {
            int n = matrix.GetLength(0);
            for (int k = 0; k < numberOftimes; k++)
            {
                for (int i = 0; i < n / 2; i++)
                {
                    for (int j = i; j < n - i - 1; j++)
                    {
                        Tile top = matrix[i, j];
                        matrix[i, j] = matrix[n - 1 - j, i];
                        matrix[n - 1 - j, i] = matrix[n - i - 1, n - 1 - j];
                        matrix[n - i - 1, n - 1 - j] = matrix[j, n - i - 1];
                        matrix[j, n - i - 1] = top;
                    }
                }
            }
        }

        private void PutNewValue()
        {
            List<Tuple<int, int>> emptySlots = new List<Tuple<int, int>>();
            for (int cRow = 0; cRow < _bRow; cRow++)
            {
                for (int cCol = 0; cCol < _bCol; cCol++)
                {
                    if (_board[cRow, cCol].Value == 0)
                    {
                        emptySlots.Add(new Tuple<int, int>(cRow, cCol));
                    }
                }
            }
            int iSlot = random.Next(0, emptySlots.Count);
            int value = random.Next(0, 100) < 95 ? 2 : 4;
            Sequence seq = DOTween.Sequence();
            _board[emptySlots[iSlot].Item1, emptySlots[iSlot].Item2].SetValue(value);
            _board[emptySlots[iSlot].Item1, emptySlots[iSlot].Item2].PutNewValue(seq);
            seq.OnComplete(() => RoundEnd());
            seq.Play();
        }

        private void RoundEnd()
        {
            _isAnimating = false;
            RecordBoardState();
            if (_isWin)
            {
                _gameState = GameState.GameWon;
            }
            else if (IsGameEnd())
            {
                _gameState = GameState.GameOver;
            }
        }

        private void RecordBoardState()
        {
            _boardState = new int[_bRow, _bCol];
            for (int cRow = 0; cRow < _bRow; cRow++)
            {
                for (int cCol = 0; cCol < _bCol; cCol++)
                {
                    _boardState[cRow, cCol] = _board[cRow, cCol].Value;
                }
            }
            _boardStateHistory.Add(_boardState);
        }

        public void Revert()
        {
            if (_boardStateHistory.Count > 1)
            {
                _boardStateHistory.RemoveAt(_boardStateHistory.Count - 1);
                var lastBoardState = _boardStateHistory.Last();
                GameStart(lastBoardState);
            }
        }

        public void GameRestart()
        {
            _gameState = GameState.GameMenu;
        }

        private bool IsGameEnd()
        {
            foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
            {
                if (GameLogic(dir, true))
                {
                    return false;
                }
            }
            return true;
        }
        public void MoveUp()
        {
            GameUpdate(Direction.Up);
        }
        public void MoveDown()
        {
            GameUpdate(Direction.Down);
        }
        public void MoveRight()
        {
            GameUpdate(Direction.Right);
        }
        public void MoveLeft()
        {
            GameUpdate(Direction.Left);
        }
    

        bool GameLogic(Direction direction, bool checkOnly = false)
        {
            bool hasUpdated = false;

            int outterCount = _bRow;
            int innerCount = _bCol;
            int rotateCount = 0;
            switch (direction)
            {
                case Direction.Up:
                    rotateCount = 1;
                    break;
                case Direction.Down:
                    rotateCount = 3;
                    break;
                case Direction.Left:
                    rotateCount = 2;
                    break;

            }
            RotateMatrixByKTimes(_board, rotateCount);

            int innerStart = innerCount - 1;
            int innerEnd = 0;

            Func<Tile[,], int, int, int> getValue = new Func<Tile[,], int, int, int>((x, i, j) => x[i, j].CurrentValue);
            Action<int, int, int, int, int> doSwap = new Action<int, int, int, int, int>((i, j, newJ, v, v2) => SwapTile((i, j), (i, newJ), v, v2));

            Func<int, bool> innerCondition = index => Math.Min(innerStart, innerEnd) <= index && index <= Math.Max(innerStart, innerEnd);

            for (int i = 0; i < outterCount; i++)
            {
                for (int j = innerStart; innerCondition(j); j--)
                {
                    if (getValue(_board, i, j) == 0)
                    {
                        continue;
                    }

                    int newJ = j;
                    do
                    {
                        newJ++;
                    }
                    while (innerCondition(newJ) && getValue(_board, i, newJ) == 0);

                    if (innerCondition(newJ) && getValue(_board, i, newJ) == getValue(_board, i, j) && _board[i, newJ].CanMerge)
                    {

                        int newValue = getValue(_board, i, newJ) * 2;
                        if (newValue == 2048)
                        {
                            _isWin = true;
                        }
                        hasUpdated = true;
                        if (!checkOnly)
                        {
                            doSwap(i, j, newJ, 0, newValue);
                            Score += newValue;
                        }
                    }
                    else
                    {
                        newJ--;
                        if (newJ != j)
                        {
                            hasUpdated = true;
                        }
                        int value = getValue(_board, i, j);
                        if (!checkOnly)
                        {
                            doSwap(i, j, newJ, 0, value);
                        }
                    }
                }
            }
            RotateMatrixByKTimes(_board, 4 - rotateCount);
            return hasUpdated;
        }

        void GameUpdate(Direction direction)
        {
            if (_isAnimating || _isWin)
            {
                return;
            }
            _isAnimating = true;
    
            foreach (Tile tile in _board)
            {
                tile.ResetStatus();
            }

            var hasUpdated = GameLogic(direction);
            Sequence seq = DOTween.Sequence();
            foreach (Tile tile in _board)
            {
                tile.DoAnim(seq);
            }
     
            if (hasUpdated)
            {
                seq.OnComplete(() => PutNewValue());
            }
            else
            {
                _isAnimating = false;
            }

            seq.Play();
        }

        void SwapTile((int, int) pos1, (int, int) pos2, int v, int newV)
        {
            var p1i = pos1.Item1;
            var p1j = pos1.Item2;
            var p2i = pos2.Item1;
            var p2j = pos2.Item2;

            var tmp = _board[p1i, p1j];
            _board[p1i, p1j] = _board[p2i, p2j];
            _board[p2i, p2j] = tmp;

            if ( _board[p2i, p2j].CurrentValue != 0 && newV != 0 && _board[p2i, p2j].CurrentValue != newV)
            {
                _board[p2i, p2j].CanMerge = false;
            }
            if (_board[p1i, p1j].NewValue != null && _board[p1i, p1j].NewValue != 0 && v == 0)
            {
                _board[p1i, p1j].AnimPosition = new Vector3(_board[p1i, p1j].CurrentPosition.x, _board[p1i, p1j].CurrentPosition.y);
            }
            _board[p1i, p1j].SetValue(v);
     
            _board[p2i, p2j].SetValue(newV);

            var tmpPos = new Vector3 (_board[p1i, p1j].CurrentPosition.x, _board[p1i, p1j].CurrentPosition.y);

            _board[p1i, p1j].SetPosition(_board[p2i, p2j].CurrentPosition);
            _board[p2i, p2j].SetPosition(tmpPos);
            _board[p1i, p1j].transform.SetAsFirstSibling();
        }


        private void DetectInput(InputAction.CallbackContext context)
        {
            var vec2 = context.ReadValue<Vector2>();
            if (vec2.x == 0)
            {
                if (vec2.y > 0)
                {
                    MoveUp();
                }
                else
                {
                    MoveDown();
                }
            }
            else
            {
                if (vec2.x < 0)
                {
                    MoveLeft();
                }   
                else
                {
                    MoveRight();
                } 
            }
        }

        private void DetectSwipe()
        {
            if (Vector3.Distance(_touchStartPos, _touchEndPos) >= _touchMinimumDistance &&
                (_touchEndTime - _touchStartTime) <= _touchMaximumTime)
            {
                Vector3 direction = _touchEndPos - _touchStartPos;
                Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;
                SwipeDirection(direction2D);
            }
        }

        private void SwipeDirection(Vector2 direction)
        {
            if (Vector2.Dot(Vector2.up, direction) > _touchDirectionThreshold)
            {
                MoveUp();
            }
            else if (Vector2.Dot(Vector2.down, direction) > _touchDirectionThreshold)
            {
                MoveDown();
            }
            else if (Vector2.Dot(Vector2.left, direction) > _touchDirectionThreshold)
            {
                MoveLeft();
            }
            else if (Vector2.Dot(Vector2.right, direction) > _touchDirectionThreshold)
            {
                MoveRight();
            }
        }

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _keyboardAction = _playerInput.actions["Keyboard"];
            _touchAction = _playerInput.actions["Touch"];
            _touchPositionAction = _playerInput.actions["Touch Position"];
        }

        private void OnEnable()
        {
            _keyboardAction.performed += DetectInput;
            _touchAction.started += StartTouchPrimary;
            _touchAction.canceled += EndTouchPrimary;
        }
        private void StartTouchPrimary(InputAction.CallbackContext context)
        {
            _touchStartPos = ScreenToWorld(Camera.main, _touchPositionAction.ReadValue<Vector2>());
            _touchStartTime = (float)context.time;
        }
        private void EndTouchPrimary(InputAction.CallbackContext context)
        {
            _touchEndPos = ScreenToWorld(Camera.main, _touchPositionAction.ReadValue<Vector2>());
            _touchEndTime = (float)context.time;
            DetectSwipe();
        }

        private void OnDisable()
        {
            _keyboardAction.performed -= DetectInput;
            _touchAction.started -= StartTouchPrimary;
            _touchAction.canceled -= EndTouchPrimary;
        }

        private Vector3 ScreenToWorld(Camera camera, Vector3 position)
        {
            position.z = camera.nearClipPlane;
            return camera.ScreenToWorldPoint(position);
        }
    }
}