using Mirror;
using UnityEngine;

public class LineDoorTask : GameTask {
    [SyncVar]
    public byte Buttons;

    private byte _buttons;

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
        int buttonsCount = Random.Range(3, 8);
        byte buttons = 0b0;
        for (int i = 0; i < buttonsCount; ++i) {
            int pos;
            do {
                pos = Random.Range(0, 8);
            } while (((buttons >> pos) & 0b1) == 1);

            buttons |= (byte) (0b1 << pos);
        }

        Buttons = buttons;

        return true;
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        ActivatorObject.Active = !player.IsHunter;
        return true;
    }

    public override void OnTaskClose(Player player) {
    }

    public override void OnTaskOpenClient() {
        _buttons = 0;
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
        // TODO(dm): Refactor.
        
        GUI.Box(new Rect(Screen.width / 2f - 256f, Screen.height / 2f - 256f, 512f, 512f), "");
        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 2; ++j) {
                int pos = i * 2 + j;
                int state = ((_buttons >> pos) & 0b1) == 1                                  ? 1 :
                            ((Buttons >> pos) & 0b1) == 1 && ((_buttons >> pos) & 0b1) == 0 ? 2 : 0;
                Texture2D tex = state == 0 ? _regularBackground : state == 1 ? _tickedBackground : _unTickedBackground;
                GUITaskUtils.SetBackground(tex);
                if (GUI.Button(new Rect(Screen.width / 2f - 248f + j * 256f, Screen.height / 2f - 248f + i * 128f, 240f, 112f), "")) {
                    if (state == 1) {
                        _buttons ^= (byte) (0b1 << pos);
                    }
                    else {
                        _buttons |= (byte) (0b1 << pos);
                    }
                }
            }
        }

        if (Buttons == _buttons && Buttons != 0) {
            SendTaskFinish();
        }

        GUITaskUtils.SetBackground(null);

        // GUI.skin.button.normal.background = 
    }
}