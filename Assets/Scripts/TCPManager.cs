using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

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
    private Action handleStart = null;
    private Action<SearchRoom> handleSearchRoom = null;

    public Action<ResponseGame> SetHandleResponsetTicTacToe { set { handleResponsetTicTacToe = value; } }
    public Action<RoomInfo> SetHandleMatchingTicTacToe { set { handleMatchingTicTacToe = value; } }
    public Action<List<RoomInfo>> SetHandleRoomList { set { handleRoomList = value; } }
    public Action<UserInfo> SetHandleUserInfo { set { handleUserInfo = value; } }
    public Action<long> SetHandlePing { set { handlePing = value; } }
    public Action SetHandleStart { set { handleStart = value; } }
    public Action<SearchRoom> SetHandleSearchRoom { set { handleSearchRoom = value; } }

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
            UnityEngine.Debug.Log("서버에 연결되었습니다.");

            _ = ReadDataAsync();

            return true;
        }
        catch (SocketException ex)
        {
            UnityEngine.Debug.LogError($"서버 연결 오류: {ex.Message}");

            return false;
        }
    }

    public async UniTask<string> Login(string _id)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(_id);
        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

        byte[] buffer = new byte[1024];
        int byteCount = await stream.ReadAsync(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, byteCount);
        Console.WriteLine($"서버 응답 수신: {response}");

        return response;
    }

    private async UniTask ReadDataAsync()
    {
        int byteCount;

        try
        {
            while ((byteCount = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);
                var resultOpcode = JsonConvert.DeserializeObject<Packet>(response);

                switch (resultOpcode.opcode)
                {
                    case Opcode.C_UserInfo:
                        var resultUserInfo = JsonConvert.DeserializeObject<RequestUserInfo>(response);
                        handleUserInfo?.Invoke(resultUserInfo.userInfo);
                        break;
                    case Opcode.C_Create_Room:
                        handleStart?.Invoke();
                        break;
                    case Opcode.C_Search_Room:
                        var resultSearchRoom = JsonConvert.DeserializeObject<SearchRoom>(response);
                        handleSearchRoom?.Invoke(resultSearchRoom);
                        break;
                    case Opcode.C_Enter_Room:
                        handleStart?.Invoke();
                        break;
                    case Opcode.C_Leave_Room:
                        handleStart?.Invoke();
                        break;
                    case Opcode.C_Room_List:
                        var resultRoomList = JsonConvert.DeserializeObject<RequestRoomList>(response);
                        handleRoomList?.Invoke(resultRoomList.roomList);
                        break;
                    case Opcode.C_Start:
                        handleStart?.Invoke();
                        break;
                    case Opcode.C_Matching:
                        var resultRoomInfo = JsonConvert.DeserializeObject<RoomInfo>(response);
                        handleMatchingTicTacToe?.Invoke(resultRoomInfo);
                        break;
                    case Opcode.C_Cancel_Matching:
                        handleStart?.Invoke();
                        break;
                    case Opcode.C_TicTacToe:
                        var responseGameResult = JsonConvert.DeserializeObject<ResponseGame>(response);
                        handleResponsetTicTacToe?.Invoke(responseGameResult);
                        break;
                    case Opcode.C_Ping:
                        await SendPongAsync();
                        HandlePong(resultOpcode.timestamp);
                        break;
                    default:
                        Console.WriteLine("알 수 없는 Opcode");
                        break;
                }
            }
        }
        catch (SocketException ex)
        {
            UnityEngine.Debug.LogError($"데이터 읽기 오류: {ex.Message}");
        }
        finally
        {
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

    public async UniTask CancelMatching()
    {
        Packet request = new Packet();
        request.opcode = Opcode.C_Cancel_Matching;
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

    public async void HandleReady(int _roomNumber, int _player)
    {
        RequestReady request = new RequestReady();
        request.opcode = Opcode.C_Ready;
        request.roomNumber = _roomNumber;
        request.player = _player;

        var convert = JsonConvert.SerializeObject(request);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async void HandleReadyCancel(int _roomNumber, int _player)
    {
        RequestReady request = new RequestReady();
        request.opcode = Opcode.C_Ready_Cancel;
        request.roomNumber = _roomNumber;
        request.player = _player;

        var convert = JsonConvert.SerializeObject(request);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async UniTask HandleCreateRoom()
    {
        Packet request = new Packet();
        request.opcode = Opcode.C_Create_Room;
        var convert = JsonConvert.SerializeObject(request);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async UniTask HandleSearchRoom(int _roomNumber)
    {
        SearchRoom request = new SearchRoom();
        request.opcode = Opcode.C_Search_Room;
        request.roomNumber = _roomNumber;

        var convert = JsonConvert.SerializeObject(request);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async UniTask HandleEnterRoom(RoomInfo _roomInfo)
    {
        EnterRoom enterRoom = new EnterRoom();
        enterRoom.opcode = Opcode.C_Enter_Room;
        enterRoom.roominfo = _roomInfo;

        var convert = JsonConvert.SerializeObject(enterRoom);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public async UniTask HandleLeaveRoom(RoomInfo _roomInfo)
    {
        LeaveRoom leaveRoom = new LeaveRoom();
        leaveRoom.opcode = Opcode.C_Leave_Room;
        leaveRoom.roominfo = _roomInfo;

        var convert = JsonConvert.SerializeObject(leaveRoom);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);

        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    private async UniTask SendPongAsync()
    {
        Packet pongPacket = new Packet { opcode = Opcode.C_Pong };
        var convert = JsonConvert.SerializeObject(pongPacket);
        byte[] messageBytes = Encoding.UTF8.GetBytes(convert);
        await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
    }

    public void HandlePong(long _serverTimestamp)
    {
        long currentTimestamp = DateTime.UtcNow.Ticks;
        long ping = (currentTimestamp - _serverTimestamp) / TimeSpan.TicksPerMillisecond;

        handlePing?.Invoke(ping);
    }
}