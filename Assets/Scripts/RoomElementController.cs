using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomElementController : MonoBehaviour
{
    private readonly string Playing = "������";
    private readonly string Waiting = "�����";

    [SerializeField] private Button enterButton = null;
    [SerializeField] private TMP_Text isPlaying = null;

    private Action callBack = null;

    private void Awake()
    {
        enterButton.onClick.AddListener(OnClickEnterButton);
    }

    private void OnClickEnterButton()
    {
        callBack?.Invoke();
        callBack = null;
    }

    public void SetRoomElement(Action _callBack, bool _isPlaying)
    {
        callBack = _callBack;
        isPlaying.text = _isPlaying == true ? Playing : Waiting;
    }
}
