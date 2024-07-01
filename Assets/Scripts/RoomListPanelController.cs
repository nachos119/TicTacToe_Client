using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanelController : MonoBehaviour
{
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button createRoomButton = null;
    [SerializeField] private RoomElementController roomElement = null;
    [SerializeField] private GameObject scrollViewContent = null;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnClickCloseButton);
        //createRoomButton.onClick.AddListener(OnClickCreateRoomButton);
    }

    public async void Show()
    {
        gameObject.SetActive(true);

        TCPManager.Instance.SetHandleRoomList = SetRoomList;
    }

    public void SetRoomList(List<RoomInfo> _roomInfos)
    {
        int count = _roomInfos.Count;
    }

    private void OnClickCloseButton()
    {
        Hide();
    }

    private void OnClickCreateRoomButton()
    {

    }

    private void EnterRoom()
    {

    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
