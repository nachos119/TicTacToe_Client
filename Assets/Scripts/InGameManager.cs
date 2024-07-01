using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : LazySingleton<InGameManager>
{
    private UserInfo userInfo = null;
    private RoomInfo roomInfo = null;

    public UserInfo SetUserInfo { get { return userInfo; } set { userInfo = value; } }
    public RoomInfo SetRoomInfo { get { return roomInfo; } set { roomInfo = value; } }

}
