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

    [SerializeField] private TMP_Text statusText = null;    // ���� ���¸� ǥ���ϴ� �ؽ�Ʈ

    [SerializeField] private Button readyButton = null;
    [SerializeField] private TMP_Text readyButtonText = null;
    [SerializeField] private GameObject ReadyObject = null;

    private InGameManager inGameManager = null;
    private TCPManager tcpManager = null;
    private List<TicTacToeElementController> ticTacToeList = null;

    private CancellationTokenSource readyCancellationTokenSource = null;

    private int[] board;        // ���� ���带 ��Ÿ���� �迭
    private int currentPlayer;  // ���� �÷��̾� (0: X, 1: O)
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
        // 1p 2p ����
        currentPlayer = inGameManager.SetRoomInfo.users[0].connectNumber == inGameManager.SetUserInfo.connectNumber ? 0 : 1;
        // timer ����
        // ������ ���������� Ȥ�� ������ �������� ó��
        // ������� ��������

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
            board[i] = -1; // �� ĭ�� -1�� �ʱ�ȭ
        }

        currentPlayer = 0; // X �÷��̾���� ����
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
        Debug.Log("�̻��{currentPlayer}");
        // �� �α� �ִ�
        await ticTacToeList[_responseGame.index].ChangeTicTacToeElement(_responseGame.player);

        // ���
        if (_responseGame.playing == true)
        {
            // ���� ���� ������
            if (currentPlayer == _responseGame.player)
            {
                Debug.Log("����{_responseGame.player}");
                dimImage.active = true;
            }
            else
            {
                dimImage.active = false;
            }

            // ������ �� �� ���� �ִ�
            if (_responseGame.delete == true)
            {
                await ticTacToeList[_responseGame.deleteIndex].ChangeTicTacToeElement(-1);
            }
        }
        else
        {
            dimImage.active = true;

            // ��������
            if (_responseGame.winner == currentPlayer)
            {
                // ���� �¸�
                Debug.Log("�¸�");
            }
            else
            {
                // ���� �¸�
                Debug.Log("����");
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
