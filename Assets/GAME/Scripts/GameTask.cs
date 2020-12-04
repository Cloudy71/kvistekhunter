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

    public GameObject[] ActiveSync;

    public GameObstacle ActivatorObject;

    public bool Closeable = true;
    public bool UnkillableInTask;

    [HideInInspector]
    public float LastVictimOpened;

    [HideInInspector]
    public float LastHunterOpened;

    private Light _light;
    private bool  _firstCome;
    private bool  _oldVictimActive;
    private bool  _oldHunterActive;

    protected virtual void Awake() {
        _light = transform.Find("TaskLight")?.GetComponent<Light>();
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

    private void OnVictimActiveChange() {
        // if (isServer) {
        //     if (ActivatorObject != null && ActivateIfVictimActive)
        //         ActivatorObject.GetComponent<GameObstacle>().Active = true;
        // }
        if (isServer) {
            foreach (GameObject task in ActiveSync) {
                task.GetComponent<GameTask>().VictimActive = VictimActive;
            }
        }

        // Debug.Log("VICTIM CHANGE0 " + name);

        if (Player.GetLocal == null)
            return;

        if (Player.GetLocal.IsHunter)
            return;
        if (_light != null) _light.enabled = VictimActive;
        // Debug.Log("VICTIM CHANGE1 " + name);
    }

    private void OnHunterActiveChange() {
        // if (isServer) {
        //     if (ActivatorObject != null && ActivateIfHunterActive)
        //         ActivatorObject.GetComponent<GameObstacle>().Active = true;
        // }

        if (isServer) {
            foreach (GameObject task in ActiveSync) {
                task.GetComponent<GameTask>().HunterActive = HunterActive;
            }
        }

        // Debug.Log("HUNTER CHANGE0 " + name);

        if (Player.GetLocal == null)
            return;

        if (!Player.GetLocal.IsHunter)
            return;
        if (_light != null) _light.enabled = HunterActive;
        // Debug.Log("HUNTER CHANGE0 " + name);
    }

    public void ForceCloseTask(Player player) {
        player.CurrentTask = null;
        player.TargetSetTask(player.connectionToClient, null);
        OnTaskForceClose(player);
    }

    public abstract bool OnTaskOpen(Player player);

    public abstract void OnTaskFinish(Player player, params object[] data);

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

    public virtual void OnGUI() {
    }

    public virtual void OnTaskGUI() {
        if (_firstCome) {
            _firstCome = false;
            OnTaskOpenClient();
        }
    }
}