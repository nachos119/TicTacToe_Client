using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] private TicTacToeElementController tacToeElementController = null;
    [SerializeField] private GameObject gamePanelObject = null;
    [SerializeField] private TMP_Text timer = null;
    [SerializeField] private TMP_Text player_1 = null;
    [SerializeField] private TMP_Text player_2 = null;

    [SerializeField] private GameObject dimImage = null;

    [SerializeField] private TMP_Text statusText; // ���� ���¸� ǥ���ϴ� �ؽ�Ʈ

    private InGameManager inGameManager = null;
    private List<TicTacToeElementController> ticTacToeList = null;

    private int[] board; // ���� ���带 ��Ÿ���� �迭
    private int currentPlayer; // ���� �÷��̾� (0: X, 1: O)

    private void Start()
    {
        inGameManager = InGameManager.Instance;

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
        if (currentPlayer == 0)
        {
            dimImage.active = false;
        }
        else
        {
            dimImage.active = true;
        }

        TCPManager.Instance.SetHandleResponsetTicTacToe = ResponseData;

        EnabledTacToeElement(true);
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
            ticTacToeList[i].SetButton(OnClickButton);
            ticTacToeList[i].ChangeTicTacToeElement(-1);
        }

        EnabledTacToeElement(false);

        board = new int[count];
        for (int i = 0; i < count; i++)
        {
            board[i] = -1; // �� ĭ�� -1�� �ʱ�ȭ
        }

        currentPlayer = 0; // X �÷��̾���� ����
        statusText.text = "Player X's Turn";
    }

    private async void OnClickButton(int _index)
    {
        dimImage.active = true;
        await TCPManager.Instance.HandleRequestTicTacToe(inGameManager.SetRoomInfo.roomNumber, _index, currentPlayer);
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
        ticTacToeList[_responseGame.index].ChangeTicTacToeElement(_responseGame.player);

        if (_responseGame.delete == true)
        {
            ticTacToeList[_responseGame.deleteIndex].ChangeTicTacToeElement(-1);
        }

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
}
