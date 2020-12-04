using Mirror;
using UnityEngine;

public class DoorsTask : GameTask {
    [SyncVar]
    public int Doors;

    public GameObject[] Controlling;
    public bool[]       WasActive;

    private int _doors;

    private Texture2D _unTickedBackground;
    private Texture2D _tickedBackground;

    protected override void Start() {
        base.Start();
        Doors = 0;
        WasActive = new bool[Controlling.Length];
        for (int i = 0; i < Controlling.Length; ++i) {
            Doors |= 1 << i;
            WasActive[i] = Controlling[i].GetComponent<GameObstacle>().Active;
        }

        _unTickedBackground = AssetLoader.GetColor(200, 64, 64);
        _tickedBackground = AssetLoader.GetColor(64, 200, 64);
    }

    public override bool OnTaskOpen(Player player) {
        int maxDoor = 0;
        for (int i = 0; i < Controlling.Length; ++i) {
            maxDoor |= 1 << i;
        }

        if (!player.IsHunter && Doors == maxDoor)
            return false;
        return true;
    }

    public override void OnTaskFinish(Player player, params object[] data) {
    }

    public override void OnTaskClose(Player player) {
    }

    public override void OnTaskStep(Player player, params object[] data) {
        base.OnTaskStep(player, data);
        if (data.Length != 1)
            return;
        int doors = (int) data[0];
        for (int i = 0; i < Controlling.Length; ++i) {
            int oldState = (Doors >> i) & 1;
            int state = (doors >> i) & 1;
            if (state == oldState)
                continue;
            LineDoorObject lineDoor = Controlling[i].GetComponent<LineDoorObject>();
            if (state == 1) {
                lineDoor.Off = false;
                lineDoor.Active = WasActive[i];
                foreach (GameObject task in lineDoor.ActivateTask) {
                    GameTask t = task.GetComponent<GameTask>();
                    t.VictimActive = !WasActive[i];
                    t.HunterActive = WasActive[i];
                }
            }
            else {
                WasActive[i] = lineDoor.Active;
                lineDoor.Off = true;
                lineDoor.Active = false;
                foreach (GameObject task in lineDoor.ActivateTask) {
                    GameTask t = task.GetComponent<GameTask>();
                    t.VictimActive = false;
                    t.HunterActive = false;
                }
            }
        }

        Doors = doors;
    }

    public override void OnTaskOpenClient() {
        _doors = Doors;
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();

        GUI.Box(new Rect(Screen.width / 2f - 256f, Screen.height / 2f - 64f, 512f, 128f), "");
        int size = Controlling.Length;
        for (int i = 0; i < size; ++i) {
            int state = ((_doors >> i) & 0b1) == 1 ? 1 : 0;
            Texture2D tex = state == 0 ? _unTickedBackground : _tickedBackground;
            GUI.skin.button.normal.background = tex;
            GUI.skin.button.hover.background = tex;
            GUI.skin.button.active.background = tex;
            if (GUI.Button(new Rect(Screen.width / 2f - (size / 2f * 64f) + i * 64f, Screen.height / 2f - 56f, 48f, 112f), "")) {
                if (state == 1) {
                    _doors ^= (byte) (0b1 << i);
                }
                else {
                    _doors |= (byte) (0b1 << i);
                }

                Player.Local.CmdTaskStep(new TaskPayload(_doors));
            }
        }

        // if (Player.GetLocal.IsHunter && _buttons == 0b11111111 || !Player.GetLocal.IsHunter && _buttons == 0) {
        //     Player.GetLocal.CmdTaskFinish(null);
        //     OnTaskFinishClient();
        // }

        GUI.skin.button.normal.background = GameAssets.DefaultUnityNormalBackground;
        GUI.skin.button.hover.background = GameAssets.DefaultUnityHoverBackground;
        GUI.skin.button.active.background = GameAssets.DefaultUnityActiveBackground;
    }
}