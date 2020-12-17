using Mirror;
using UnityEngine;

public class NumbersLocalTask : GameLocalTask {
    public int Amount;

    [SyncVar]
    public string Numbers;

    private string _current;

    public override bool OnTaskOpen(Player player) {
        if (!base.OnTaskOpen(player))
            return false;

        Numbers = "";
        for (int i = 0; i < Amount; ++i) {
            Numbers += Random.Range(0, 10).ToString();
        }

        return true;
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        string code = (string) data[0];
        if (code != Numbers) {
            SendTaskResponse(player, false);
            return false;
        }

        return base.OnTaskFinish(player, data);
    }

    public override void OnTaskOpenClient() {
        _current = "";
    }

    public override void OnTaskResponse(params object[] data) {
        base.OnTaskResponse(data);
        if (!(bool) data[0])
            _current = "";
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
        GUI.Box(new Rect(Screen.width / 2f - 160f, Screen.height / 2f - 240f, 320f, 480f), "");
        GUI.Box(new Rect(Screen.width / 2f - 144f, Screen.height / 2f - 224f, 288f, 32f), "");
        GUI.Label(new Rect(Screen.width / 2f - 136f, Screen.height / 2f - 250f, 272f, 32f), Numbers);
        GUI.Label(new Rect(Screen.width / 2f - 136f, Screen.height / 2f - 224f, 272f, 32f), _current);
        for (int i = 0; i < 9; ++i) {
            float x = Screen.width / 2f - 144f + i % 3 * 102f;
            float y = Screen.height / 2f - 172f + Mathf.Floor(i / 3f) * 102f;
            if (GUI.Button(new Rect(x, y, 86f, 86f), (i + 1).ToString())) {
                _current += (i + 1).ToString();
            }
        }

        if (GUI.Button(new Rect(Screen.width / 2f - 144f, Screen.height / 2f + 134f, 86f, 86f), "Clear")) {
            _current = "";
        }

        if (GUI.Button(new Rect(Screen.width / 2f - 42f, Screen.height / 2f + 134f, 86f, 86f), "0")) {
            _current += "0";
        }

        if (GUI.Button(new Rect(Screen.width / 2f + 60f, Screen.height / 2f + 134f, 86f, 86f), "OK")) {
            SendTaskFinish(_current);
            // TODO(dm)
        }
    }
}