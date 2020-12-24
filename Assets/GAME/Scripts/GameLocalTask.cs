using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class GameLocalTask : GameTask {
    public enum LocalTaskType {
        Common,
        Long,
        Short,
        Additive
    }

    public bool SelfActive;
    public int  Damage;

    public LocalTaskType Type;

    public Dictionary<Player, bool> PlayerFinished;

    private Dictionary<Player, Coroutine>     _coroutines;
    private Dictionary<Player, CustomPayload> _data;

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
        _coroutines = new Dictionary<Player, Coroutine>();
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
        foreach (GameTask gameTask in ActivateOnFinish) {
            if (gameTask is GameLocalTask gameLocalTask) {
                player.TaskList.Add(gameLocalTask);
            }
        }

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

        // foreach (GameObject activeSync in ActiveSync) {
        //     activeSync.GetComponent<GameLocalTask>().SelfActive = SelfActive;
        // }
    }

    public void ForceNotify() {
        OnVictimActiveChange();
    }

    protected void RegisterCoroutine(Player player, Coroutine coroutine) {
        if (_coroutines.ContainsKey(player))
            ClearCoroutine(player);
        _coroutines[player] = coroutine;
    }

    protected void ClearCoroutine(Player player) {
        if (!_coroutines.ContainsKey(player))
            return;
        StopCoroutine(_coroutines[player]);
        _coroutines.Remove(player);
    }

    protected void SetData(Player player, CustomPayload payload) {
        SetDataInjection(player, payload);
        player.TargetSetTaskData(player.connectionToClient, payload);
    }

    protected CustomPayload GetData(Player player) {
        if (!_data.ContainsKey(player))
            return null;
        return _data[player];
    }

    public void SetDataInjection(Player player, CustomPayload payload) {
        _data[player] = payload;
    }
}