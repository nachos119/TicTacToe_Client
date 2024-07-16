using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingManager : LazySingleton<PingManager>
{
    private PingPanelCountroller pingPanelCountroller = null;

    public PingPanelCountroller SetPingPanel { get { return pingPanelCountroller; } set { pingPanelCountroller = value; } }
}
