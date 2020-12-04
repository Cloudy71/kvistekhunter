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

    protected override void Start() {
        base.Start();
        _usage = new Dictionary<Player, float>();
        _trapLight = transform.Find("TrapLight").GetComponent<Light>();
        OnTrappedChanged(Trapped, Trapped);
    }

    public override bool OnTaskOpen(Player player) {
        if (player.IsHunter) {
            if (Trapped)
                return false;
            if (!SetUsed(player))
                return false;
            Trapped = true;
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

    public override void OnTaskFinish(Player player, params object[] data) {
        GameObject path = (GameObject) data[0];
        Vector3 position = path.transform.position;
        player.transform.position = position;
        player.RpcSetPosition(position);
        SetUsed(player, true);
        path.GetComponent<VentTask>().SetUsed(player, true);
        player.RpcPlayAnimation("UnVent");
    }

    private void OnTrappedChanged(bool oldValue, bool newValue) {
        _trapLight.enabled = newValue;
    }

    public override void OnTaskClose(Player player) {
        player.RpcPlayAnimation("UnVent");
    }

    public override void OnTaskOpenClient() {
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();

        foreach (GameObject pathObject in Path) {
            Vector3 pathPos = pathObject.transform.position;
            Vector3 realArrowPos = Vector3.MoveTowards(transform.position, pathPos, 2f);
            Vector3 screenPos = GameManager.GetCamera().WorldToScreenPoint(realArrowPos);
            if (GUI.Button(new Rect(screenPos.x - 16f, Screen.height - screenPos.y - 16f, 32f, 32f), "")) {
                Player.GetLocal.CmdTaskFinish(new TaskPayload(pathObject));
                OnTaskFinishClient();
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