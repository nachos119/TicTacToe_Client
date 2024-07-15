using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanelController : MonoBehaviour
{
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button createRoomButton = null;
    [SerializeField] private Button searchRoomButton = null;

    [SerializeField] private RoomElementController roomElement = null;
    [SerializeField] private GameObject scrollViewContent = null;

    [SerializeField] private RoomInfoPanelController roomInfoPanelController = null;
    [SerializeField] private SearchRoomPanelController searchRoomPanelController = null;

    private TCPManager tcpManager = null;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        searchRoomButton.onClick.AddListener(OnClickSearchRoomButton);
        //createRoomButton.onClick.AddListener(OnClickCreateRoomButton);

        tcpManager = TCPManager.Instance;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        searchRoomPanelController.SetSearchRoomPanel(SearchRoom);

        roomInfoPanelController.gameObject.SetActive(false);
        searchRoomPanelController.gameObject.SetActive(false);

        tcpManager.SetHandleRoomList = SetRoomList;
        tcpManager.SetHandleSearchRoom = SearchRoomAsync;

    }

    public void SetRoomList(List<RoomInfo> _roomInfos)
    {
        int count = _roomInfos.Count;

        // 방리스트 업
        // 게임중인지도 알아야함
    }

    private void OnClickCloseButton()
    {
        if (roomInfoPanelController.gameObject.activeSelf == true)
        {
            roomInfoPanelController.gameObject.SetActive(false);
        }
        else if (searchRoomPanelController.gameObject.activeSelf == true)
        {
            searchRoomPanelController.gameObject.SetActive(false);
        }
        else
        {
            Hide();
        }
    }

    private void OnClickCreateRoomButton()
    {

    }

    private void OnClickSearchRoomButton()
    {
        searchRoomPanelController.gameObject.SetActive(true);
    }

    private void EnterRoom(RoomInfo _roomInfo)
    {
        // 방정보 띄우기
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private async void SearchRoom(int _roomNumber)
    {
        await tcpManager.HandleSearchRoom(_roomNumber);

        // load바 만들기
    }

    private void SearchRoomAsync(SearchRoom _searchRoom)
    {
        EnterRoom(_searchRoom.roominfo);
    }
}