using System.Collections.Generic;
using Mirror;

public abstract class GameLocalTask : GameTask {
    public enum LocalTaskType {
        Common,
        Long,
        Short
    }

    public bool SelfActive;
    public int  Damage;

    public LocalTaskType Type;

    public Dictionary<Player, bool> PlayerFinished;

    protected override void Awake() {
        base.Awake();
        if (isServer) {
            PlayerFinished = new Dictionary<Player, bool>();
            foreach (KeyValuePair<int, NetworkConnectionToClient> connection in NetworkServer.connections) {
                PlayerFinished.Add(connection.Value.identity.GetComponent<Player>(), false);
            }
        }
    }

    protected override void Start() {
        base.Start();
        VictimActive = true;
        HunterActive = false;
        // OnVictimActiveChange();
        // OnHunterActiveChange();
    }

    public override bool OnTaskOpen(Player player) {
        if (!player.TaskList.Contains(this))
            return false;

        return true;
    }

    public override void OnTaskClose(Player player) {
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        player.TaskList.Remove(this);
        player.SynchronizeTaskList();
        GameManager.Instance.SetHuntersHealth(GameManager.Instance.StatusHuntersHealth - Damage);
        return true;
    }

    public override void OnTaskFinishClient() {
        base.OnTaskFinishClient();
        OnVictimActiveChange();
    }

    protected override void OnVictimActiveChange() {
        base.OnVictimActiveChange();
        if (Player.GetLocal == null || Player.GetLocal.IsHunter)
            return;

        SelfActive = Player.GetLocal.TaskList.Contains(this);
        light.enabled = SelfActive;
    }

    public void ForceNotify() {
        OnVictimActiveChange();
    }
}