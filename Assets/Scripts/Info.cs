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
    C_Create_Room = 1,
    C_Join_Room = 2,
    C_Search_Room = 3,
    C_Ready = 4,
    C_Start = 5,
    C_Room_List = 6,
    C_Room_Entry = 7,
    C_Matching = 8,
    C_Cancle_Matching = 9,

    SendMessage = 10,

    C_UserInfo = 14,

    C_TicTacToe = 20,


    C_Ping = 50,
    C_Pong = 51,
}

public class Packet
{
    public Opcode opcode;
    public string message;
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
    public RoomInfo currentRoom;

    // 연결된 번호
    public int connectNumber;
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
    public List<RoomInfo> roomList { get; set; }
}

public class RequestUserInfo : Packet
{
    public UserInfo userInfo { get; set; }
}