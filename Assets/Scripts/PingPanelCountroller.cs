using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PingPanelCountroller : MonoBehaviour
{
    [SerializeField] private TMP_Text pingText = null;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        TCPManager.Instance.SetHandlePing = UpdatePing;
    }

    private void UpdatePing(long _ping)
    {
        pingText.text = $"{_ping} ms";
    }
}
