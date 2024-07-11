using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoPanelController : MonoBehaviour
{
    [SerializeField] private Button enterRoomButton = null;
    [SerializeField] private TMP_Text roomNumberText = null;
    [SerializeField] private TMP_Text roomManagerPlayerText = null;
    [SerializeField] private TMP_Text playerText = null;

    private Action<int> callBack = null;
    private int roomNumber;

    private void Awake()
    {
        enterRoomButton.onClick.AddListener(OnClickEnterButton);
    }

    public void SetAction(Action<int> _callBack)
    {
        callBack = _callBack;
    }

    public void SetRoomInfo(int _roomNumber)
    {
        roomNumber = _roomNumber;
        roomNumberText.text = $"방 번호 : {_roomNumber}";
    }

    public void SetRoomInfo(int _roomNumber, string _managerPlayer, string _player)
    {
        roomNumber = _roomNumber;
        roomNumberText.text = $"방 번호 : {_roomNumber}";
        roomManagerPlayerText.text = $"{_managerPlayer}";
        playerText.text = $"{_player}";
    }

    private void OnClickEnterButton()
    {
        callBack?.Invoke(roomNumber);
    }
}
