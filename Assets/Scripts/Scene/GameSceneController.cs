using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] private TicTacToeElementController tacToeElementController = null;
    [SerializeField] private GameObject gamePanelObject = null;
    [SerializeField] private TMP_Text timer = null;
    [SerializeField] private TMP_Text player_1 = null;
    [SerializeField] private TMP_Text player_2 = null;

    [SerializeField] private GameObject dimImage = null;

    [SerializeField] private TMP_Text statusText = null;    // 게임 상태를 표시하는 텍스트

    [SerializeField] private Button readyButton = null;
    [SerializeField] private TMP_Text readyButtonText = null;
    [SerializeField] private GameObject ReadyObject = null;

    private InGameManager inGameManager = null;
    private TCPManager tcpManager = null;
    private List<TicTacToeElementController> ticTacToeList = null;

    private CancellationTokenSource readyCancellationTokenSource = null;

    private int[] board;        // 게임 보드를 나타내는 배열
    private int currentPlayer;  // 현재 플레이어 (0: X, 1: O)
    private bool isReady;

    private void Start()
    {
        inGameManager = InGameManager.Instance;
        tcpManager = TCPManager.Instance;

        readyButton.onClick.AddListener(OnClickReadyButton);

        ResetGame();

        SetRoom();
    }

    private void SetRoom()
    {
        // 1p 2p 세팅
        currentPlayer = inGameManager.SetRoomInfo.users[0].connectNumber == inGameManager.SetUserInfo.connectNumber ? 0 : 1;
        // timer 세팅
        // 상대방이 강종했을때 혹은 연결이 끊겼을때 처리
        // 어떤식으로 진행할지

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
            board[i] = -1; // 빈 칸을 -1로 초기화
        }

        currentPlayer = 0; // X 플레이어부터 시작
        statusText.text = "Player X's Turn";

        ReadyObject.SetActive(true);
        isReady = false;
        ChangeReadyButton();
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
        dimImage.active = _responseGame.player == currentPlayer ? true : false;
        Debug.Log("이사람{currentPlayer}");
        // 돌 두기 애니
        await ticTacToeList[_responseGame.index].ChangeTicTacToeElement(_responseGame.player);

        // 결과
        if (_responseGame.playing == true)
        {
            // 아직 게임 진행중
            if (currentPlayer == _responseGame.player)
            {
                Debug.Log("요사람{_responseGame.player}");
                dimImage.active = true;
            }
            else
            {
                dimImage.active = false;
            }

            // 이전에 둔 돌 제거 애니
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
                // 유저 승리
                Debug.Log("승리");
            }
            else
            {
                // 상대방 승리
                Debug.Log("졌다");
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
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
    }
}
