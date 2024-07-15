using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchRoomPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchInputField = null;
    [SerializeField] private Button searchRoomButton = null;

    private Action<int> callBack = null;
    private int roomNumber;

    private void Awake()
    {
        searchRoomButton.onClick.AddListener(OnClickSearchButton);
    }

    public void SetSearchRoomPanel(Action<int> _callBack)
    {
        callBack = _callBack;
    }

    private void OnClickSearchButton()
    {
        roomNumber = int.Parse(searchInputField.text);
        callBack?.Invoke(roomNumber);
    }
}
