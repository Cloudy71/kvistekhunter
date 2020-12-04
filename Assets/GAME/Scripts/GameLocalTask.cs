using System.Collections.Generic;
using Mirror;

public abstract class GameLocalTask : GameTask {
    public enum LocalTaskType {
        Common,
        Long,
        Short
    }

    public bool SelfActive;

    public LocalTaskType Type;

    public Dictionary<Player, bool> PlayerFinished;

    protected override void Awake() {
        base.Awake();
        if (isServer) {
            PlayerFinished = new Dictionary<Player, bool>();
            foreach (KeyValuePair<int, NetworkConnectionToClient> connection in NetworkServer.connections) {
                PlayerFinished.Add(connection.Value.identity.GetComponent<Player>(), false);
            }

            // switch (Type) {
            //     case LocalTaskType.Common:
            //         foreach (Player player in PlayerActive.Keys) {
            //             PlayerActive[player] = true;
            //         }
            //
            //         break;
            //     case LocalTaskType.Long: case LocalTaskType.Short:
            //         
            //         break;
            // }
        }
    }

    protected override void Start() {
        base.Start();
        VictimActive = true;
        HunterActive = false;
        OnVictimActiveChange();
        OnHunterActiveChange();
    }

    public override bool OnTaskOpen(Player player) {
        if (!player.TaskList.Contains(this))
            return false;

        return true;
    }

    public override void OnTaskClose(Player player) {
    }

    public override void OnTaskFinish(Player player, params object[] data) {
        player.TaskList.Remove(this);
        player.SynchronizeTaskList();
    }

    protected override void OnVictimActiveChange() {
        base.OnVictimActiveChange();
        if (Player.GetLocal == null || Player.GetLocal.IsHunter)
            return;

        SelfActive = Player.GetLocal.TaskList.Contains(this);
        light.enabled = SelfActive;
    }
}