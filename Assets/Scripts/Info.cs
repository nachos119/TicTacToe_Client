using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userInfo
{
    public string name;
    public roomInfo roomInfo;
}

public class roomInfo
{
    public int roomNumber;
    public bool isPlaying;
}
public enum Opcode
{
    C_Login = 0,

    C_UserInfo = 1,

    C_Create_Room = 10,
    C_Join_Room = 11,
    C_Search_Room = 12,
    C_Enter_Room = 13,
    C_Leave_Room = 14,
    C_Room_List = 15,

    C_Ready = 21,
    C_Start = 22,
    C_Ready_Cancel = 23,

    C_Matching = 30,
    C_Cancel_Matching = 31,

    C_TicTacToe = 40,

    C_Ping = 50,
    C_Pong = 51,

    SendMessage = 100,
}

public class Packet
{
    public Opcode opcode;
    public string message;
    public long timestamp;
    public long ping;
}

public class TcpMessage : Packet
{
    public string messages;
}

public class RoomInfo : Packet
{
    public int roomNumber;
    public List<UserInfo> users;
    public bool isPlaying;
    public int[] board;              // 보드 상태 저장
    public Queue<int> playerSelectQueue;
}

public class UserInfo
{
    public string name;

    // 연결된 번호
    public int connectNumber;
    public bool isReady;
    public bool isMatching;
    public bool hasPonged;
    public long pingTimestamp;
}

public class RequestGame : Packet
{
    public int roomNumber;
    public int index;
    public int player;
}

public class ResponseGame : Packet
{
    public int roomNumber;
    public int player;
    public int index;
    public bool playing;
    public int winner;
    public bool delete;
    public int deleteIndex;
}
public class RequestRoomList : Packet
{
    public List<RoomInfo> roomList;
}

public class RequestUserInfo : Packet
{
    public UserInfo userInfo;
}

public class SearchRoom : Packet
{
    public int roomNumber;
    public bool existRoom;
    public RoomInfo roominfo;
}

public class CancelMatching : Packet
{
    public bool isCancel;
}

public class LeaveRoom : Packet
{
    public UserInfo userInfo;
    public RoomInfo roominfo;
}

public class EnterRoom : Packet
{
    public RoomInfo roominfo;
}

public class RequestReady : Packet
{
    public int roomNumber;
    public int player;
}