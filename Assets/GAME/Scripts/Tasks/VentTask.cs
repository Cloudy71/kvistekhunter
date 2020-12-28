using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class VentTask : GameTask {
    [SyncVar(hook = "OnTrappedChanged")]
    public bool Trapped;

    public float        Cooldown;
    public GameObject[] Path;

    private Dictionary<Player, float> _usage;
    private Light                     _trapLight;
    private float                     _inVentSince;
    private float                     _trappedSince;

    protected override void Start() {
        base.Start();
        _usage = new Dictionary<Player, float>();
        _trapLight = transform.Find("TrapLight").GetComponent<Light>();
        OnTrappedChanged(Trapped, Trapped);
    }

    protected override void Update() {
        base.Update();
        if (!isServer)
            return;

        if (Trapped && NetworkTime.time >= _trappedSince + 20f) {
            Trapped = false;
        }
    }

    public override bool OnTaskOpen(Player player) {
        if (player.IsHunter) {
            if (Trapped)
                return false;
            if (!SetUsed(player))
                return false;
            Trapped = true;
            _trappedSince = (float) NetworkTime.time;
            return false;
        }

        if (!SetUsed(player))
            return false;
        player.RpcPlayAnimation("Vent");
        if (Trapped) {
            Trapped = false;
            player.Kill(player.gameObject);
            player.RpcSetPosition(transform.position);
            return false;
        }

        player.RpcSetPosition(transform.position);

        return true;
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        GameObject path = (GameObject) data[0];
        Vector3 position = path.transform.position;
        player.transform.position = position;
        player.RpcSetPosition(position);
        SetUsed(player, true);
        path.GetComponent<VentTask>().SetUsed(player, true);
        player.RpcPlayAnimation("UnVent");

        return true;
    }

    private void OnTrappedChanged(bool oldValue, bool newValue) {
        _trapLight.enabled = newValue;
    }

    public override void OnTaskClose(Player player) {
        player.RpcPlayAnimation("UnVent");
    }

    public override void OnTaskOpenClient() {
        _inVentSince = Time.time;
    }

    public override void OnTaskUpdateClient() {
        base.OnTaskUpdateClient();
        for (var i = 0; i < Path.Length; ++i) {
            if (Input.GetKeyDown((i + 1).ToString())) {
                SendTaskFinish(Path[i]);
            }
        }

        if (Time.time >= _inVentSince + 5f) {
            SendTaskClose();
        }
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
        // TODO(dm): Refactor.

        int i = 1;
        foreach (GameObject pathObject in Path) {
            Vector3 pathPos = pathObject.transform.position;
            Vector3 realArrowPos = Vector3.MoveTowards(transform.position, pathPos, 2f);
            Vector3 screenPos = GameManager.GetCamera().WorldToScreenPoint(realArrowPos);
            if (GUI.Button(new Rect(screenPos.x - 16f, Screen.height - screenPos.y - 16f, 32f, 32f), (i++).ToString())) {
                SendTaskFinish(pathObject);
            }
        }
    }

    public bool SetUsed(Player player, bool force = false) {
        if (_usage.ContainsKey(player)) {
            if (!force && NetworkTime.time < _usage[player] + Cooldown)
                return false;
            _usage[player] = (float) NetworkTime.time;
            return true;
        }

        _usage.Add(player, (float) NetworkTime.time);
        return true;
    }
}