using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameManager : LazySingleton<InGameManager>
{
    private UserInfo userInfo = null;
    private RoomInfo roomInfo = null;

    private bool isSingleGame = false;

    public UserInfo SetUserInfo { get { return userInfo; } set { userInfo = value; } }
    public RoomInfo SetRoomInfo { get { return roomInfo; } set { roomInfo = value; } }
    public bool SetSingleGame { get { return isSingleGame; } set { isSingleGame = value; } }

    public int CheckWin(int[] _board, int[][] _winPatterns)
    {
        foreach (var pattern in _winPatterns)
        {
            if (_board[pattern[0]] != -1 &&
                _board[pattern[0]] == _board[pattern[1]] &&
                _board[pattern[1]] == _board[pattern[2]])
            {
                return _board[pattern[0]];
            }
        }

        return -1;
    }
}
