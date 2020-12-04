using System;
using UnityEngine;

public class GameScreenDrawer : MonoBehaviour {
    public static GameScreenDrawer Instance;

    public bool Intro;
    public bool Outro;

    private float _introStart;
    private float _outroStart;
    private float _imageY;
    private float _imageA;
    private float _bgA;

    private Texture2D _colorBlack;

    private void Start() {
        Instance = this;

        _introStart = 0f;
        _outroStart = 0f;

        _colorBlack = AssetLoader.GetColor(0, 0, 0);
    }

    private void Update() {
        if (Intro) {
            if (_introStart.Equals(0f))
                _introStart = Time.time;

            float perc = (Time.time - _introStart) / 5f;
            if (perc <= .5f) {
                _imageY = perc / .5f;
                _bgA = 1f;
                _imageA = 1f;
            }
            else {
                _imageY = 1f;
                _bgA = 1f - (perc - .5f) / .5f;
                _imageA = _bgA;
            }

            if (perc >= 1f) {
                Intro = false;
                _introStart = 0f;
            }
        }
    }

    private void OnGUI() {
        if (Player.GetLocal == null)
            return;

        if (Intro) {
            float y = 64f - _imageY * 64f;
            Texture2D tex = Player.GetLocal.IsHunter ? GameAssets.Instance.IntroHunterSprite.texture : GameAssets.Instance.IntroVictimSprite.texture;
            GUI.color = new Color(1f, 1f, 1f, _bgA);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _colorBlack);
            GUI.color = new Color(1f, 1f, 1f, _imageA);
            GUI.DrawTexture(new Rect(Screen.width / 2f - 320f, Screen.height / 2f - 43f - y, 640f, 86f), tex);
            GUI.color = Color.white;
        }
    }
}