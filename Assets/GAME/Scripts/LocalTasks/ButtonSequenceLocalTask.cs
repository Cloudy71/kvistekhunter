using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSequenceLocalTask : GameLocalTask {
    public int Buttons;

    [SyncVar]
    public int TickedButtons;

    private int   _tickedButtons;
    private float _timeOpened;

    private Texture2D _regularBackground;
    private Texture2D _unTickedBackground;
    private Texture2D _tickedBackground;

    protected override void Start() {
        base.Start();
        _regularBackground = AssetLoader.GetColor(200, 200, 200);
        _unTickedBackground = AssetLoader.GetColor(200, 64, 64);
        _tickedBackground = AssetLoader.GetColor(64, 200, 64);
    }

    public override bool OnTaskOpen(Player player) {
        if (!base.OnTaskOpen(player))
            return false;

        GenerateNewSequence();

        return true;
    }

    public void GenerateNewSequence() {
        int newNumber = 0;
        for (int i = 0; i < Buttons; ++i) {
            int pos;
            do {
                pos = Random.Range(0, 25);
            } while (((newNumber >> pos) & 0b1) == 1);

            newNumber |= 1 << pos;
        }

        TickedButtons = newNumber;
    }

    public override void OnTaskStep(Player player, params object[] data) {
        base.OnTaskStep(player, data);
        int buttons = (int) data[0];
        bool failed = false;
        int finished = 0;
        for (int i = 0; i < 25; ++i) {
            if (((buttons >> i) & 0b1) == 1 && ((TickedButtons >> i) & 0b1) == 0) {
                failed = true;
                break;
            }

            if (((buttons >> i) & 0b1) == 1 && ((TickedButtons >> i) & 0b1) == 1)
                finished++;
        }

        if (failed) {
            GenerateNewSequence();
            SendTaskResponse(player, false);
            return;
        }

        if (finished == Buttons) {
            SendTaskResponse(player, true);
        }
    }

    public override void OnTaskResponseClient(params object[] data) {
        base.OnTaskResponseClient(data);
        bool status = (bool) data[0];
        if (status)
            SendTaskFinish(_tickedButtons);
        else {
            _timeOpened = Time.time + 1f;
            _tickedButtons = 0;
        }
    }

    public override void OnTaskOpenClient() {
        _timeOpened = Time.time;
        _tickedButtons = 0;
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();

        bool preview = Time.time >= _timeOpened && Time.time <= _timeOpened + 1f;
        bool error = Time.time < _timeOpened;

        GUI.Box(new Rect(Screen.width / 2f - 256f, Screen.height / 2f - 256f, 512f, 512f), "");
        for (int i = 0; i < 5; ++i) {
            for (int j = 0; j < 5; ++j) {
                float x = Screen.width / 2f - 256f + 8f + j * 99f;
                float y = Screen.height / 2f - 256f + 8f + i * 99f;
                int index = i * 5 + j;
                bool ticked = preview && ((TickedButtons >> index) & 0b1) == 1 || ((_tickedButtons >> index) & 0b1) == 1;
                Texture2D tex = error ? _unTickedBackground : ticked ? _tickedBackground : _regularBackground;
                GUITaskUtils.SetBackground(tex);
                if (GUI.Button(new Rect(x, y, 90f, 90f), "")) {
                    if (!preview && !error) {
                        _tickedButtons |= 1 << index;
                        SendTaskStep(_tickedButtons);
                    }
                }
            }
        }

        GUITaskUtils.SetBackground(null);
    }
}