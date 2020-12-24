using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LightsTask : GameTask {
    [SyncVar]
    public bool Buttons;

    private byte _buttons;

    private Texture2D _unTickedBackground;
    private Texture2D _tickedBackground;

    protected override void Start() {
        base.Start();
        Buttons = true;

        _unTickedBackground = AssetLoader.GetColor(200, 64, 64);
        _tickedBackground = AssetLoader.GetColor(64, 200, 64);
    }

    public override bool OnTaskOpen(Player player) {
        if (player.IsHunter && Buttons ||
            !player.IsHunter && !Buttons)
            return false;
        return true;
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        GameStatus.Instance.LightsOff = !player.IsHunter;
        Buttons = player.IsHunter;
        if (player.IsHunter) {
            VictimActive = true;
            HunterActive = false;
        }
        else {
            VictimActive = false;
            HunterActive = true;
        }

        return true;
    }

    public override void OnTaskClose(Player player) {
    }

    public override void OnTaskOpenClient() {
        _buttons = (byte) (Buttons ? 0b11111111 : 0);
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
        // TODO(dm): Refactor.
        
        GUI.Box(new Rect(Screen.width / 2f - 256f, Screen.height / 2f - 64f, 512f, 128f), "");
        for (int i = 0; i < 8; ++i) {
            int state = ((_buttons >> i) & 0b1) == 1 ? 1 : 0;
            Texture2D tex = state == 0 ? _unTickedBackground : _tickedBackground;
            GUITaskUtils.SetBackground(tex);
            if (GUI.Button(new Rect(Screen.width / 2f - 248f + i * 64f, Screen.height / 2f - 56f, 48f, 112f), "")) {
                if (state == 1) {
                    _buttons ^= (byte) (0b1 << i);
                }
                else {
                    _buttons |= (byte) (0b1 << i);
                }
            }
        }

        if (Player.GetLocal.IsHunter && _buttons == 0b11111111 || !Player.GetLocal.IsHunter && _buttons == 0) {
            Player.GetLocal.CmdTaskFinish(null);
            SendTaskFinish();
            OnTaskFinishClient();
        }

        GUITaskUtils.SetBackground(null);
    }
}