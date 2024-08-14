using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour
{
    private readonly string opponentText = "Opponent";
    private readonly string turnText = "Turn";
    private readonly string myText = "My";

    [SerializeField] private TicTacToeElementController tacToeElementController = null;
    [SerializeField] private GameObject gamePanelObject = null;
    [SerializeField] private TMP_Text timer = null;
    [SerializeField] private TMP_Text player_1 = null;
    [SerializeField] private TMP_Text player_2 = null;

    [SerializeField] private GameObject dimImage = null;

    [SerializeField] private TMP_Text statusText = null;

    [SerializeField] private Button readyButton = null;
    [SerializeField] private TMP_Text readyButtonText = null;
    [SerializeField] private GameObject ReadyObject = null;

    [SerializeField] private GameObject resultObject = null;
    [SerializeField] private Image resultImage = null;
    [SerializeField] private Sprite winImage = null;
    [SerializeField] private Sprite loseImage = null;

    [SerializeField] private Button resultButton = null;

    private CancellationTokenSource resultCancellationTokenSource = null;

    private InGameManager inGameManager = null;
    private TCPManager tcpManager = null;
    private TimerManager timerManager = null;
    private List<TicTacToeElementController> ticTacToeList = null;

    private CancellationTokenSource readyCancellationTokenSource = null;

    private int[] board;
    private int currentPlayer;
    private bool isReady;
    private bool isSingleGame;
    private Queue<int> gameSelectQueue = null;
    private int[][] winPatterns = null;

    private void Start()
    {
        inGameManager = InGameManager.Instance;
        tcpManager = TCPManager.Instance;
        timerManager = TimerManager.Instance;

        readyButton.onClick.AddListener(OnClickReadyButton);
        resultButton.onClick.AddListener(OnClickResultButton);

        timerManager.SetUpdateTimerUI = UpdateTimer;

        isSingleGame = inGameManager.SetSingleGame;

        ResetGame();

        SetRoom();
    }

    private void SetRoom()
    {
        if (isSingleGame == false)
        {
            currentPlayer = inGameManager.SetRoomInfo.users[0].connectNumber == inGameManager.SetUserInfo.connectNumber ? 0 : 1;

            if (currentPlayer == 0)
            {
                statusText.text = $"{myText} {turnText}";
                player_1.text = myText;
                player_2.text = opponentText;
            }
            else
            {
                statusText.text = $"{opponentText} {turnText}";
                player_1.text = opponentText;
                player_2.text = myText;
            }

            tcpManager.SetHandleResponsetTicTacToe = ResponseData;
            tcpManager.SetHandleStart = TicTacToeStart;

        }
        else
        {
            currentPlayer = 0;
            statusText.text = $"{myText} {turnText}";
            player_1.text = myText;
            player_2.text = $"Computer";

            SetSingleGame();
        }

        EnabledTacToeElement(true);

        dimImage.SetActive(true);
    }

    private void ResetGame()
    {
        dimImage.SetActive(true);

        if (ticTacToeList == null)
        {
            ticTacToeList = new List<TicTacToeElementController>();
            for (int i = 0; i < 9; i++)
            {
                ticTacToeList.Add(GameObject.Instantiate(tacToeElementController, gamePanelObject.transform));
            }
        }

        int count = ticTacToeList.Count;
        for (int i = 0; i < count; i++)
        {
            int index = i;
            ticTacToeList[i].SetIndex = index;
            if (isSingleGame == false)
            {
                ticTacToeList[i].SetButton(OnClickElementButton);
            }
            else
            {
                ticTacToeList[i].SetButton(OnClickSingleGameElementButton);
            }
            ticTacToeList[i].ResetTicTacToeElement();
        }

        EnabledTacToeElement(false);

        board = new int[count];
        for (int i = 0; i < count; i++)
        {
            board[i] = -1;
        }

        currentPlayer = 0;

        ReadyObject.SetActive(true);
        isReady = false;
        readyButtonText.text = $"Ready";
        if (isSingleGame == false)
            ChangeReadyButton();

        resultObject.SetActive(false);
    }

    private void SetSingleGame()
    {
        gameSelectQueue = new Queue<int>();

        winPatterns = new int[][]
        {
                new int[] { 0, 1, 2 },
                new int[] { 3, 4, 5 },
                new int[] { 6, 7, 8 },
                new int[] { 0, 3, 6 },
                new int[] { 1, 4, 7 },
                new int[] { 2, 5, 8 },
                new int[] { 0, 4, 8 },
                new int[] { 2, 4, 6 }
        };

        readyButtonText.text = $"Start";
    }

    private async void OnClickElementButton(int _index)
    {
        dimImage.SetActive(true);
        await tcpManager.HandleRequestTicTacToe(inGameManager.SetRoomInfo.roomNumber, _index, currentPlayer);
    }

    private void OnClickSingleGameElementButton(int _index)
    {
        dimImage.SetActive(true);
        gameSelectQueue.Enqueue(_index);
        PlayingGame(_index);
    }
    private void EnabledTacToeElement(bool _Enabled)
    {
        int count = ticTacToeList.Count;
        for (int i = 0; i < count; i++)
        {
            ticTacToeList[i].GetButton.enabled = _Enabled;
        }
    }

    private async void ResponseData(ResponseGame _responseGame)
    {
        timerManager.StopTimer();

        dimImage.SetActive(_responseGame.player == currentPlayer ? true : false);
        Debug.Log("이사람{currentPlayer}");
        await ticTacToeList[_responseGame.index].ChangeTicTacToeElement(_responseGame.player);

        // 결과
        if (_responseGame.playing == true)
        {
            timerManager.StartTimer();

            if (currentPlayer == _responseGame.player)
            {
                Debug.Log("요사람{_responseGame.player}");
                statusText.text = $"{opponentText} {turnText}";
            }
            else
            {
                dimImage.SetActive(false);
                statusText.text = $"{myText} {turnText}";
            }

            if (_responseGame.delete == true)
            {
                await ticTacToeList[_responseGame.deleteIndex].ChangeTicTacToeElement(-1);
            }
        }
        else
        {
            EndGame(_responseGame.winner == currentPlayer ? true : false);
        }
    }

    private async void PlayingGame(int _index)
    {
        timerManager.StopTimer();

        await ticTacToeList[_index].ChangeTicTacToeElement(currentPlayer);
        board[_index] = currentPlayer;

        var result = inGameManager.CheckWin(board, winPatterns);

        if (result != -1)
        {
            EndGame(true);
        }
        else
        {
            if (gameSelectQueue.Count >= 6)
            {
                var index = gameSelectQueue.Dequeue();
                await ticTacToeList[index].ChangeTicTacToeElement(-1);
                board[index] = -1;
            }

            timerManager.StartTimer();

            statusText.text = $"Computer {turnText}";

            var computerIndex = MiniMax();

            await ticTacToeList[computerIndex].ChangeTicTacToeElement(1);
            board[computerIndex] = 1;

            gameSelectQueue.Enqueue(computerIndex);

            var resultComputer = inGameManager.CheckWin(board, winPatterns);

            if (resultComputer != -1)
            {
                EndGame(false);
            }

            if (gameSelectQueue.Count >= 6)
            {
                var index = gameSelectQueue.Dequeue();
                await ticTacToeList[index].ChangeTicTacToeElement(-1);
                board[index] = -1;
            }

            dimImage.SetActive(false);
            statusText.text = $"{myText} {turnText}";
        }
    }

    private void OnClickReadyButton()
    {
        if (isSingleGame == false)
        {
            isReady = !isReady;
            ChangeReadyButton();
        }
        else
        {
            TicTacToeStart();
        }
    }

    private void ChangeReadyButton()
    {
        readyCancellationTokenSource = new CancellationTokenSource();

        readyButton.enabled = false;

        if (isReady == true)
        {
            tcpManager.HandleReady(inGameManager.SetRoomInfo.roomNumber, currentPlayer);
            readyButtonText.text = $"Cancel";
        }
        else
        {
            tcpManager.HandleReadyCancel(inGameManager.SetRoomInfo.roomNumber, currentPlayer);
            readyButtonText.text = $"Ready";
        }

        ActiveReadyButton(readyCancellationTokenSource.Token);
    }

    private async void ActiveReadyButton(CancellationToken _token)
    {
        await UniTask.Delay(2000);

        if (!_token.IsCancellationRequested)
            readyButton.enabled = true;
    }

    private void TicTacToeStart()
    {
        readyCancellationTokenSource?.Cancel();

        ReadyObject.SetActive(false);

        if (currentPlayer == 0)
        {
            dimImage.SetActive(false);
        }

        timerManager.StartTimer();
    }

    private void OnClickResultButton()
    {
        LoadScene();
    }

    private async void ResultUniTask(CancellationToken _token)
    {
        await UniTask.Delay(5000, _token.IsCancellationRequested);

        if (!_token.IsCancellationRequested)
            LoadScene();
    }

    private void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    private void UpdateTimer(float _time)
    {
        timer.text = $"{_time}";
    }

    private void OnDestroy()
    {
        timerManager.StopTimer();
    }

    private void EndGame(bool _win)
    {
        dimImage.SetActive(true);

        // 게임종료
        if (_win == true)
        {
            Debug.Log("승리");
            resultImage.sprite = winImage;
        }
        else
        {
            Debug.Log("졌다");
            resultImage.sprite = loseImage;
        }

        resultObject.SetActive(true);
        resultCancellationTokenSource = new CancellationTokenSource();
        ResultUniTask(resultCancellationTokenSource.Token);
    }

    private int MiniMax()
    {
        int bestMove = -1;
        int bestValue = int.MinValue;
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == -1)
            {
                board[i] = 1;
                int moveValue = Minimax(board, false);
                board[i] = -1;

                if (moveValue > bestValue)
                {
                    bestMove = i;
                    bestValue = moveValue;
                }
            }
        }

        return bestMove;
    }

    public int Minimax(int[] _board, bool _isMax)
    {
        int score = Evaluate(_board);

        if (score == 10 || score == -10)
            return score;
        if (IsBoardFull(_board))
            return 0;

        if (_isMax)
        {
            int best = int.MinValue;
            for (int i = 0; i < 9; i++)
            {
                if (_board[i] == -1)
                {
                    _board[i] = 1;
                    best = Math.Max(best, Minimax(_board, false));
                    _board[i] = -1;
                }
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] == -1)
                {
                    board[i] = 0;
                    best = Math.Min(best, Minimax(board, true));
                    board[i] = -1;
                }
            }
            return best;
        }
    }

    public int Evaluate(int[] _board)
    {
        int winner = inGameManager.CheckWin(_board, winPatterns);

        if (winner == 1)
            return 10;
        else if (winner == 0)
            return -10;

        return 0;
    }

    public bool IsBoardFull(int[] _board)
    {
        for (int i = 0; i < 9; i++)
        {
            if (_board[i] == -1)
                return false;
        }
        return true;
    }
}