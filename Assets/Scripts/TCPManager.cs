using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class TCPManager : LazySingleton<TCPManager>
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];

    private Action<ResponseGame> handleResponsetTicTacToe = null;
    private Action<RoomInfo> handleMatchingTicTacToe = null;
    private Action<List<RoomInfo>> handleRoomList = null;
    private Action<UserInfo> handleUserInfo = null;
    private Action<long> handlePing = null;

    public Action<ResponseGame> SetHandleResponsetTicTacToe { set { handleResponsetTicTacToe = value; } }
    public Action<RoomInfo> SetHandleMatchingTicTacToe { set { handleMatchingTicTacToe = value; } }
    public Action<List<RoomInfo>> SetHandleRoomList { set { handleRoomList = value; } }
    public Action<UserInfo> SetHandleUserInfo { set { handleUserInfo = value; } }
    public Action<long> SetHandlePing { set { handlePing = value; } }

    private static Stopwatch pingStopwatch = new Stopwatch();
    private static int pingTimeoutMilliseconds = 5000;
    private static CancellationTokenSource pingCancellationTokenSource;

    public async UniTask<bool> TcpConnectAsync(string _ipAddress, int _port)
    {
        if (client == null)
        {
            client = new TcpClient();
        }

        try
        {
            await client.ConnectAsync(_ipAddress, _port);
            stream = client.GetStream();
            UnityEngine.Debug.Log("������ ����Ǿ����ϴ�.");

            // �����κ��� �����͸� �񵿱������� �б� �����մϴ�.
            _ = ReadDataAsync();

            return true;
        }
        catch (SocketException ex)
        {
            UnityEngine.Debug.LogError($"���� ���� ����: {ex.Message}");

            return false;
        }
    }

    public async UniTask<string> Login(string _id)
    {
        // �ӽ� ���߿� �����ؾ��� �޽��� ����
        byte[] messageBytes = Encoding.UTF8.GetBytes(_id);
        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
        Console.WriteLine($"�޽��� ����: {_id}");

        // ���� ����
        byte[] buffer = new byte[1024];
        int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, byteCount);
        Console.WriteLine($"���� ���� ����: {response}");

        return response;
    }

    private async UniTask ReadDataAsync()
    {
        int byteCount;

        // �� �޽��� �ֱ������� ������
        _ = SendPingAsync(stream);

        try
        {
            while ((byteCount = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);
                Console.WriteLine($"���� �޽���: {response}");

                var resultOpcode = JsonConvert.DeserializeObject<Packet>(response);
                Console.WriteLine($"���� �޽���: {resultOpcode.opcode}");

                switch (resultOpcode.opcode)
                {
                    case Opcode.C_Room_List:
                        var resultRoomList = JsonConvert.DeserializeObject<RequestRoomList>(response);
                        handleRoomList?.Invoke(resultRoomList.roomList);
                        break;
                    case Opcode.C_Matching:
                        var resultRoomInfo = JsonConvert.DeserializeObject<RoomInfo>(response);
                        handleMatchingTicTacToe?.Invoke(resultRoomInfo);
                        break;
                    case Opcode.C_TicTacToe:
                        var responseGameResult = JsonConvert.DeserializeObject<ResponseGame>(response);
                        handleResponsetTicTacToe?.Invoke(responseGameResult);
                        break;
                    case Opcode.C_UserInfo:
                        var resultUserInfo = JsonConvert.DeserializeObject<RequestUserInfo>(response);
                        handleUserInfo?.Invoke(resultUserInfo.userInfo);
                        break;
                    case Opcode.C_Pong:
                        HandlePong();
                        break;
                    default:
                        Console.WriteLine("�� �� ���� Opcode�Դϴ�.");
                        break;
                }
            }
        }
        catch (SocketException ex)
        {
            UnityEngine.Debug.LogError($"������ �б� ����: {ex.Message}");
        }
        finally
        {
            // ��Ʈ���� Ŭ���̾�Ʈ�� �����մϴ�.
            stream?.Close();
            client?.Close();
        }
    }

    public async UniTask Matching()
    {
        Packet request = new Packet();
        request.opcode = Opcode.C_Matching;
        var convert = JsonConvert.SerializeObject(request);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async UniTask CancleMatching()
    {
        Packet request = new Packet();
        request.opcode = Opcode.C_Cancle_Matching;
        var convert = JsonConvert.SerializeObject(request);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async UniTask HandleRequestTicTacToe(int _room, int _index, int _player)
    {
        RequestGame request = new RequestGame();
        request.opcode = Opcode.C_TicTacToe;
        request.roomNumber = _room;
        request.index = _index;
        request.player = _player;

        var convert = JsonConvert.SerializeObject(request);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);
        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async UniTask PlayerInfoAsync()
    {
        Packet request = new Packet();
        request.opcode = Opcode.C_UserInfo;

        var convert = JsonConvert.SerializeObject(request);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);
        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async UniTask SendPingAsync(NetworkStream stream)
    {
        while (true)
        {
            pingCancellationTokenSource = new CancellationTokenSource();

            var pingPacket = new Packet { opcode = Opcode.C_Ping };
            var requestJson = JsonConvert.SerializeObject(pingPacket);
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

            pingStopwatch.Restart(); // �� ���� �ð� ��� ����

            await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
            Console.WriteLine("�� �޽��� ����");

            // Ÿ�Ӿƿ� ����
            _ = UniTask.Delay(pingTimeoutMilliseconds, cancellationToken: pingCancellationTokenSource.Token).ContinueWith(() =>
            {
                if (!pingCancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine("�� ������ ���� ���߽��ϴ�. ������ �����մϴ�.");
                    client.Close();
                    Environment.Exit(0);
                }
            });

            await UniTask.Delay(pingTimeoutMilliseconds); // 5�ʸ��� �� �޽��� ����
        }
    }

    public void HandlePong()
    {
        pingStopwatch.Stop();
        long ping = pingStopwatch.ElapsedMilliseconds;
        Console.WriteLine($"�� �޽����� �����߽��ϴ�. ��: {ping}ms");

        // �߰������� ó���� ������ �ִٸ� ���⿡ �ۼ�
        handlePing?.Invoke(ping);

        // Ÿ�Ӿƿ� ���
        pingCancellationTokenSource.Cancel();
    }
}
