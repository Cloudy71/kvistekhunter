using UnityEngine;

public class GeneratorTask : GameTask {
    public CodeLocalTask codeTask;

    private float _timeCodeDisplayed;

    public override bool OnTaskOpen(Player player) {
        return true;
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        return true;
    }

    public override void OnTaskClose(Player player) {
    }

    public override void OnTaskStep(Player player, params object[] data) {
        base.OnTaskStep(player, data);
        int type = (int) data[0];
        if (type == 0) {
            codeTask.GenerateNewNumbers();
            SendTaskResponse(player, 0);
        }
    }

    public override void OnTaskResponseClient(params object[] data) {
        base.OnTaskResponseClient(data);
        int type = (int) data[0];
        if (type == 0) {
            _timeCodeDisplayed = Time.time;
        }
    }

    public override void OnTaskOpenClient() {
        _timeCodeDisplayed = float.NegativeInfinity;
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
        // TODO(dm): Refactor.
        
        GUI.Box(new Rect(Screen.width / 2f - 256f, Screen.height / 2f - 256f, 512f, 512f), "");

        #region Numbers code

        GUI.Box(new Rect(Screen.width   / 2f - 248f, Screen.height / 2f - 248f, 140f, 76f), "");
        GUI.Label(new Rect(Screen.width / 2f - 240f, Screen.height / 2f - 240f, 124f, 20f), "Number generator");
        if (GUI.Button(new Rect(Screen.width / 2f - 240f, Screen.height / 2f - 220f, 124f, 20f), "Generate new code")) {
            SendTaskStep(0);
        }

        if (Time.time >= _timeCodeDisplayed && Time.time <= _timeCodeDisplayed + 3f) {
            GUI.Label(new Rect(Screen.width / 2f - 240f, Screen.height / 2f - 200f, 124f, 20f), codeTask.Numbers);
        }

        #endregion
        
        
    }
}