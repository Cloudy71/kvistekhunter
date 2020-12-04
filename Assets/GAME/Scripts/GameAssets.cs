using System;
using UnityEngine;

public class GameAssets : MonoBehaviour {
    public static GameAssets Instance;

    public static Texture2D DefaultUnityNormalBackground;
    public static Texture2D DefaultUnityHoverBackground;
    public static Texture2D DefaultUnityActiveBackground;

    public Material MaterialPlayerLocal;
    public Material MaterialPlayerOther;
    public Material MaterialPlayerHunter;

    public Sprite IntroVictimSprite;
    public Sprite IntroHunterSprite;

    public RenderTexture AdminTableRenderTexture;

    private void Awake() {
        Instance = this;
    }

    private void OnGUI() {
        if (DefaultUnityNormalBackground != null)
            return;

        DefaultUnityNormalBackground = GUI.skin.button.normal.background;
        DefaultUnityHoverBackground = GUI.skin.button.hover.background;
        DefaultUnityActiveBackground = GUI.skin.button.active.background;
    }
}