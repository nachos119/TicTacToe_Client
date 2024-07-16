
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LobbySceneController : MonoBehaviour
{
    [SerializeField] private RoomListPanelController roomListPanelController = null;
    [SerializeField] private Button roomListButton = null;
    [SerializeField] private Button matchingButton = null;

    [SerializeField] private PingPanelCountroller pingPanelCountroller = null;

    private Canvas roomListPanel = null;
    private TCPManager tcpManager = null;
    private InGameManager inGameManager = null;

    private void Awake()
    {
        roomListButton.onClick.AddListener(OnClickRoomListButton);
        matchingButton.onClick.AddListener(OnClickMatchingButton);

        tcpManager = TCPManager.Instance;
        inGameManager = InGameManager.Instance;

        tcpManager.SetHandleMatchingTicTacToe = Matched;
        tcpManager.SetHandleUserInfo = SetUserInfo;
    }

    private async void Start()
    {
        await tcpManager.PlayerInfoAsync();

        if (PingManager.Instance.SetPingPanel == null)
            PingManager.Instance.SetPingPanel = GameObject.Instantiate(pingPanelCountroller);
    }

    private async void OnClickMatchingButton()
    {
        await tcpManager.Matching();
    }

    private async void OnClickCancelMatchingButton()
    {
        await tcpManager.CancelMatching();
    }

    private void Matched(RoomInfo _roomInfo)
    {
        inGameManager.SetRoomInfo = _roomInfo;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    private void OnClickRoomListButton()
    {
        if (roomListPanel == null)
        {
            GameObject.Instantiate(roomListPanelController);
        }
        roomListPanelController.Show();
    }

    private void SetUserInfo(UserInfo _userInfo)
    {
        inGameManager.SetUserInfo = _userInfo;
    }
}
