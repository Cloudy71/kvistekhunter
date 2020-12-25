using Mirror;
using UnityEngine;

// Author: Cloudy
public class CodeLocalTask : GameLocalTask {
    public int Amount;

    [SyncVar]
    public string Numbers;

    private string _current;

    protected override void Start() {
        base.Start();
        Numbers = "";
    }

    public override bool OnTaskOpen(Player player) {
        if (!base.OnTaskOpen(player))
            return false;

        // Numbers = "";
        // for (int i = 0; i < Amount; ++i) {
        //     Numbers += Random.Range(0, 10).ToString();
        // }

        return true;
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        string code = (string) data[0];
        if (Numbers == "" || code != Numbers) {
            SendTaskResponse(player, false);
            return false;
        }

        return base.OnTaskFinish(player, data);
    }

    public override void OnTaskOpenClient() {
        if (Numbers == "")
            _current = "Code hasn't been generated yet. Generate one on generator.";
        else
            _current = "";
    }

    public override void OnTaskResponseClient(params object[] data) {
        base.OnTaskResponseClient(data);
        if (!(bool) data[0])
            _current = "";
    }

    public void GenerateNewNumbers() {
        Numbers = "";
        for (int i = 0; i < Amount; ++i) {
            Numbers += Random.Range(0, 10).ToString();
        }
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
        // TODO(dm): Refactor.
        
        GUI.Box(new Rect(Screen.width / 2f - 160f, Screen.height / 2f - 240f, 320f, 480f), "");
        GUI.Box(new Rect(Screen.width / 2f - 144f, Screen.height / 2f - 224f, 288f, 40f), "");
        // GUI.Label(new Rect(Screen.width / 2f - 136f, Screen.height / 2f - 250f, 272f, 32f), Numbers);
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(Screen.width / 2f - 136f, Screen.height / 2f - 224f, 272f, 40f), _current);
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        for (int i = 0; i < 9; ++i) {
            float x = Screen.width  / 2f - 144f + i % 3               * 102f;
            float y = Screen.height / 2f - 172f + Mathf.Floor(i / 3f) * 102f;
            if (GUI.Button(new Rect(x, y, 86f, 86f), (i + 1).ToString())) {
                if (Numbers != "")
                    _current += (i + 1).ToString();
            }
        }

        if (GUI.Button(new Rect(Screen.width / 2f - 144f, Screen.height / 2f + 134f, 86f, 86f), "Clear")) {
            if (Numbers != "")
                _current = "";
        }

        if (GUI.Button(new Rect(Screen.width / 2f - 42f, Screen.height / 2f + 134f, 86f, 86f), "0")) {
            if (Numbers != "")
                _current += "0";
        }

        if (GUI.Button(new Rect(Screen.width / 2f + 60f, Screen.height / 2f + 134f, 86f, 86f), "OK")) {
            if (Numbers != "")
                SendTaskFinish(_current);
        }
    }
}