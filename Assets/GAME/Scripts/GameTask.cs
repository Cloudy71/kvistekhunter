using Mirror;
using UnityEngine;

public abstract class GameTask : NetworkBehaviour {
    [SyncVar]
    public bool VictimActive;

    [SyncVar]
    public bool HunterActive;

    public float VictimCooldown;
    public float HunterCooldown;
    public bool  SetCooldownOnStep;

    public GameTask[] ActiveSync;

    public GameTask[] ActivateOnFinish;

    public GameObstacle ActivatorObject;

    public bool Closeable = true;
    public bool UnkillableInTask;

    [HideInInspector]
    public float LastVictimOpened;

    [HideInInspector]
    public float LastHunterOpened;

    protected Light light;

    private bool _firstCome;
    private bool _oldVictimActive;
    private bool _oldHunterActive;

    public bool IsFirstCome => !_firstCome;

    protected virtual void Awake() {
        light = transform.Find("TaskLight")?.GetComponent<Light>();
        _firstCome = true;
    }

    protected virtual void Start() {
        LastVictimOpened = -VictimCooldown;
        LastHunterOpened = -HunterCooldown;

        _oldVictimActive = VictimActive;
        _oldHunterActive = HunterActive;
        OnVictimActiveChange();
        OnHunterActiveChange();
    }

    protected virtual void Update() {
        if (VictimActive != _oldVictimActive) {
            _oldVictimActive = VictimActive;
            OnVictimActiveChange();
        }

        if (HunterActive != _oldHunterActive) {
            _oldHunterActive = HunterActive;
            OnHunterActiveChange();
        }
    }

    protected virtual void OnVictimActiveChange() {
        if (isServer) {
            foreach (GameTask task in ActiveSync) {
                task.VictimActive = VictimActive;
            }
        }

        if (Player.GetLocal == null || Player.GetLocal.IsHunter)
            return;

        if (light != null) light.enabled = VictimActive;
    }

    protected virtual void OnHunterActiveChange() {
        if (isServer) {
            foreach (GameTask task in ActiveSync) {
                task.HunterActive = HunterActive;
            }
        }

        if (Player.GetLocal == null || !Player.GetLocal.IsHunter)
            return;

        if (light != null) light.enabled = HunterActive;
    }

    public void ForceCloseTask(Player player) {
        player.CurrentTask = null;
        player.TargetSetTask(player.connectionToClient, null);
        OnTaskForceClose(player);
    }

    public abstract bool OnTaskOpen(Player player);

    public abstract bool OnTaskFinish(Player player, params object[] data);

    public abstract void OnTaskClose(Player player);

    public virtual void OnTaskStep(Player player, params object[] data) {
    }

    public virtual void OnTaskForceClose(Player player) {
    }

    public abstract void OnTaskOpenClient();

    public virtual void OnTaskFinishClient() {
        _firstCome = true;
    }

    public virtual void OnTaskCloseClient() {
        _firstCome = true;
    }

    public virtual void OnTaskUpdateClient() {
    }

    public virtual void OnTaskResponseClient(params object[] data) {
    }

    public void SendTaskStep(params object[] data) {
        Player.Local.CmdTaskStep(new CustomPayload(data));
    }

    public void SendTaskFinish(params object[] data) {
        Player.Local.CmdTaskFinish(new CustomPayload(data));
    }

    public void SendTaskClose() {
        OnTaskCloseClient();
        Player.Local.CmdTaskClose();
    }

    public void SendTaskResponse(Player player, params object[] data) {
        player.TargetTaskResponse(player.connectionToClient, new CustomPayload(data));
    }

    public virtual void OnGUI() {
    }

    public virtual void OnTaskGUI() {
        if (_firstCome) {
            _firstCome = false;
            OnTaskOpenClient();
        }
    }
}