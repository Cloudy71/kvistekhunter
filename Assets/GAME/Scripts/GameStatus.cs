using System;
using Mirror;
using UnityEngine;

public class GameStatus : NetworkBehaviour {
    public static GameStatus Instance;
    
    [SyncVar]
    public bool LightsOff = false;

    private void Start() {
        Instance = this;
    }
}