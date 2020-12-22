using UnityEngine;

public class GUITaskUtils {
    public static void SetBackground(Texture2D background) {
        if (background == null) {
            GUI.skin.button.normal.background = GameAssets.DefaultUnityNormalBackground;
            GUI.skin.button.hover.background = GameAssets.DefaultUnityHoverBackground;
            GUI.skin.button.active.background = GameAssets.DefaultUnityActiveBackground;
            return;
        }
        GUI.skin.button.normal.background = background;
        GUI.skin.button.hover.background = background;
        GUI.skin.button.active.background = background;
    }
}