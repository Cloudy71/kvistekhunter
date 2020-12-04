using UnityEngine;

public class AdminTableTask : GameTask {
    public override bool OnTaskOpen(Player player) {
        player.SeesEveryone = true;
        return true;
    }

    public override void OnTaskFinish(Player player, params object[] data) {
    }

    public override void OnTaskClose(Player player) {
        player.SeesEveryone = false;
    }

    public override void OnTaskOpenClient() {
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();

        float width = Screen.width;
        float height = Screen.height;
        if (height < width) {
            width = height;
        }
        else {
            height = width;
        }

        GUIUtility.RotateAroundPivot(-90f, new Vector2(Screen.width / 2f, Screen.height / 2f));
        GUI.DrawTexture(new Rect(Screen.width / 2f - width / 2f, Screen.height / 2f - height / 2f, width, height), GameAssets.Instance.AdminTableRenderTexture);
        GUIUtility.RotateAroundPivot(90f, new Vector2(Screen.width / 2f, Screen.height / 2f));
    }
}