using Cysharp.Threading.Tasks;
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

    private void Start()
    {
        inGameManager = InGameManager.Instance;
        tcpManager = TCPManager.Instance;
        timerManager = TimerManager.Instance;

        readyButton.onClick.AddListener(OnClickReadyButton);
        resultButton.onClick.AddListener(OnClickResultButton);

        timerManager.SetUpdateTimerUI = UpdateTimer;

        ResetGame();

        SetRoom();
    }

    private void SetRoom()
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

        EnabledTacToeElement(true);

        dimImage.active = true;
    }

    private void ResetGame()
    {
        dimImage.active = true;

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
            ticTacToeList[i].SetButton(OnClickElementButton);
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
        ChangeReadyButton();

        resultObject.SetActive(false);
    }

    private async void OnClickElementButton(int _index)
    {
        dimImage.active = true;
        await tcpManager.HandleRequestTicTacToe(inGameManager.SetRoomInfo.roomNumber, _index, currentPlayer);
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

        dimImage.active = _responseGame.player == currentPlayer ? true : false;
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
                dimImage.active = false;
                statusText.text = $"{myText} {turnText}";
            }

            if (_responseGame.delete == true)
            {
                await ticTacToeList[_responseGame.deleteIndex].ChangeTicTacToeElement(-1);
            }
        }
        else
        {
            dimImage.active = true;

            // 게임종료
            if (_responseGame.winner == currentPlayer)
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
    }

    private void OnClickReadyButton()
    {
        isReady = !isReady;
        ChangeReadyButton();
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
        readyCancellationTokenSource.Cancel();

        ReadyObject.SetActive(false);


        if (currentPlayer == 0)
        {
            dimImage.active = false;
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
}
