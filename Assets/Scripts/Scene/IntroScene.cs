using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    private readonly string ip = "127.0.0.1";
    private readonly int port = 5000;

    private TCPManager tcpManager = null;

    [SerializeField] private Button button = null;

    private void Awake()
    {
        button.onClick.AddListener(OnClickButton);
    }

    private async void OnClickButton()
    {
        tcpManager = TCPManager.Instance;
        var connect = await tcpManager.TcpConnectAsync(ip, port);

        if (connect == true)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }
        else
        {

        }
    }
}
